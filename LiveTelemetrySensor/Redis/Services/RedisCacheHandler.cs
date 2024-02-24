using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using NetTopologySuite.Utilities;
using NRedisStack.DataTypes;
using PdfExtractor.Models.Requirement;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisCacheHandler
    {

        
        //private const long RETENTION_MARGIN_IN_MILLIS = 2000;

        private RedisCacheService _redisCaheService;
        public RedisCacheHandler(RedisCacheService cacheService)
        {
            _redisCaheService = cacheService;
            _redisCaheService.FlushAll();
        }

        //Stores all parameters with duration
        public void AddRelevantRequirements(BaseSensor sensor)
        {
            if(sensor is ParameterLiveSensor) 
                _redisCaheService.CreateTimeSeries(sensor.SensedParamName);
            foreach (SensorRequirement requirement in sensor.AdditionalRequirements)
                RequirementToTimeseries(requirement);
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach (var telemetryParameter in
                teleFrame.Parameters.Where(param => _redisCaheService.HasTimeSeries(param.Name)))
            {
                CacheParameter(
                    telemetryParameter.Name,
                    new TimeSeriesTuple(teleFrame.TimeStamp, double.Parse(telemetryParameter.Value))
                    );
            }
        }

        //Checks if the requirement has been met for the duration - iterates over all cached values
        //If a duration requirement already met - only checks current latest param value to match requirement
        // sometimes delete all samples?
        public RequirementStatus UpdateRequirementStatus(SensorRequirement sensorRequirement)
        {
            ReuploadToRedis(sensorRequirement);

            var parameterTimeSeries = _redisCaheService.GetStoredObject<RedisTimeSeries>(sensorRequirement.ParameterName);
            TimeSeriesTuple? latestSample = parameterTimeSeries.GetLastestSample();
            if (latestSample == null)
                sensorRequirement.RequirementStatus = RequirementStatus.REQUIREMENT_NOT_MET;
            else if (sensorRequirement.Duration == null)
                sensorRequirement.RequirementStatus = RequirementStatusHelper.FromBool(sensorRequirement.Requirement.RequirementMet(latestSample.Val));
            else
            {
                RequirementRange? durationRequirementRange = sensorRequirement.Duration.Requirement as RequirementRange;
                bool maximumRetentionReached = parameterTimeSeries.RetentionReached(retentionTime: sensorRequirement.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM));

                if (durationRequirementRange == null || durationRequirementRange.EndValue == double.PositiveInfinity)
                {
                    sensorRequirement.RequirementStatus = HandleLowerBoundedDuration(sensorRequirement, latestSample, maximumRetentionReached);
                }
                else if(durationRequirementRange.Value == double.NegativeInfinity)
                {
                    sensorRequirement.RequirementStatus = HandleUpperBoundedDuration(sensorRequirement, maximumRetentionReached);
                }
                else
                {
                    bool minimumRetentionReached = parameterTimeSeries.RetentionReached(retentionTime: sensorRequirement.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM));
                    sensorRequirement.RequirementStatus = HandleRangeDuration(sensorRequirement, latestSample, maximumRetentionReached, minimumRetentionReached);   
                }
            }

            return sensorRequirement.RequirementStatus;
        }

        private RequirementStatus HandleLowerBoundedDuration(SensorRequirement requirement, TimeSeriesTuple latestSample, bool maximumRetentionReached) 
        {
            if (requirement.RequirementStatus == RequirementStatus.REQUIREMENT_MET)
            {
                // if requirement not met, delete all samples
                return RequirementStatusHelper.FromBool(requirement.Requirement.RequirementMet(latestSample.Val));
            }
            else if (maximumRetentionReached)
            {
                // Values up to the start value
                IEnumerable<TimeSeriesTuple> samples = GetSamples(
                    requirement.ParameterName,
                    requirement.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM)
                    );

                return SamplesMeetRequirement(samples, requirement.Requirement, false);
            }
            else
                return RequirementStatus.REQUIREMENT_NOT_MET;
        }

        private RequirementStatus HandleUpperBoundedDuration(SensorRequirement requirement, bool maximumRetenetionReached)
        {
            if (maximumRetenetionReached)
            {
                // Values up to the start value
                IEnumerable<TimeSeriesTuple> samples = GetSamples(
                    requirement.ParameterName,
                    requirement.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM)
                    );
                return SamplesMeetRequirement(samples, requirement.Requirement, true);
            }
            else
                return RequirementStatus.REQUIREMENT_MET;
        }

        private RequirementStatus HandleRangeDuration(SensorRequirement requirement, TimeSeriesTuple latestSample, bool maximumRetentionReached, bool minimumRetentionReached)
        {
            if (maximumRetentionReached)
            {
              
                // Values up to the start value
                IEnumerable<TimeSeriesTuple> upToValueSamples = GetSamples(
                    requirement.ParameterName,
                    requirement.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                    );

                // Up until value - has to be true
                if (SamplesMeetRequirement(upToValueSamples, requirement.Requirement, false) == RequirementStatus.REQUIREMENT_NOT_MET)
                    return RequirementStatus.REQUIREMENT_NOT_MET;
                else
                {
                    // Values above the start value up to the end value
                    IEnumerable<TimeSeriesTuple> upToEndValueSamples = GetSamples(
                        requirement.ParameterName,
                        requirement.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM),
                        requirement.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                        );

                    // Value to end value - mustn't all be true
                    return SamplesMeetRequirement(upToEndValueSamples, requirement.Requirement, true);
                }
                
            }
            else if (minimumRetentionReached)
            {
                if (requirement.RequirementStatus == RequirementStatus.REQUIREMENT_MET)
                {
                    return RequirementStatusHelper.FromBool(requirement.Requirement.RequirementMet(latestSample.Val));
                }
                else
                {
                    // Values up to the start value
                    IEnumerable<TimeSeriesTuple> samples = GetSamples(
                        requirement.ParameterName,
                        requirement.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                        );
                    return SamplesMeetRequirement(samples, requirement.Requirement, false);
                }
            }
            else
                return RequirementStatus.REQUIREMENT_NOT_MET;
        }

        private void RequirementToTimeseries(SensorRequirement sensor)
        {
            _redisCaheService.CreateTimeSeries(
                    sensor.ParameterName,
                    sensor.Duration == null ? 1 : sensor.Duration.RetentionTime()
                    );
        }

        private void ReuploadToRedis(SensorRequirement sensor)
        {
            if (!_redisCaheService.ContainsKey(sensor.ParameterName.ToLower()))
                RequirementToTimeseries(sensor);
        }
        
        private RequirementStatus SamplesMeetRequirement(IEnumerable<TimeSeriesTuple> samples, RequirementParam requirement, bool reverseRequirement)
        {
            bool requirementBroken = samples.Any(sample => !requirement.RequirementMet(sample.Val));
            requirementBroken = reverseRequirement ? !requirementBroken : requirementBroken;
            return RequirementStatusHelper.FromBool(!requirementBroken);
        }

        private void CacheParameter(string parameterName, TimeSeriesTuple sample)
        {
            _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName).Add(
                sample.Time, sample.Val
                );
        }

        private IEnumerable<TimeSeriesTuple> GetSamples(string parameterName, long retentionLength, long offsetTimestamp = 0)
        {
            var redisTimeSeries = _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName);
            TimeSeriesTuple currentSample = redisTimeSeries.GetLastestSample();
            IEnumerable<TimeSeriesTuple> samplesToReturn = redisTimeSeries.GetRange(
                currentSample.Time - retentionLength,
                currentSample.Time - offsetTimestamp
                );

            var nextSampleAfterRetnention = redisTimeSeries.NextSampleAfterRetention(retentionLength);
            // If RetentionReached is true, nextSampleAfterRetention shouldn't be null and the total time length of the samples should be bigger than retention length after insertion of latestDeletedSample
            if (nextSampleAfterRetnention != null)
                samplesToReturn = samplesToReturn.Prepend(nextSampleAfterRetnention);

            return samplesToReturn;

        }


    }
}
