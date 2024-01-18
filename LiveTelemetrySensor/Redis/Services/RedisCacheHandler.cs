using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
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
        public void AddRelevantRequirements(LiveSensor sensor)
        {
            foreach (SensorRequirement requirement in sensor.AdditionalRequirements)
                SensorToTimeseries(requirement);
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach (var telemetryParameter in
                teleFrame.Parameters.Where(param => _redisCaheService.HasTimeSeries(param.Name.ToLower())))
            {
                CacheParameter(
                    telemetryParameter.Name,
                    new TimeSeriesTuple(teleFrame.TimeStamp, double.Parse(telemetryParameter.Value))
                    );
            }
        }

        //Checks if the requirement has been met for the duration - iterates over all cached values
        //If a duration requirement already met - only checks current latest param value to match requirement
        public DurationStatus UpdateDurationStatus(SensorRequirement sensor)
        {
            ReuploadToRedis(sensor);

            var parameterTimeSeries = _redisCaheService.GetStoredObject<RedisTimeSeries>(sensor.ParameterName.ToLower());
            TimeSeriesTuple? latestSample = parameterTimeSeries.GetLastestSample();
            Debug.Assert(latestSample != null);

            if (sensor.Duration == null)
                sensor.DurationStatus = DurationStatusHelper.FromBool(sensor.Requirement.RequirementMet(latestSample.Val));
            else
            {
                RequirementRange? durationRequirementRange = sensor.Duration.RequirementParam as RequirementRange;
                bool maximumRetentionReached = parameterTimeSeries.RetentionReached(retentionTime: sensor.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM));
        

                if (durationRequirementRange == null || durationRequirementRange.EndValue == double.PositiveInfinity)
                {
                    if (sensor.DurationStatus == DurationStatus.REQUIREMENT_MET)
                    {
                        // if requirement not met, delete all samples
                        sensor.DurationStatus = DurationStatusHelper.FromBool(sensor.Requirement.RequirementMet(latestSample.Val));
                    }
                    else if (maximumRetentionReached)
                    {
                        // Values up to the start value
                        IEnumerable<TimeSeriesTuple> samples = GetSamples(
                            sensor.ParameterName,
                            sensor.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM)
                            );

                        sensor.DurationStatus = SamplesMeetRequirement(samples, sensor.Requirement, false);
                    }
                    else
                        sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
                }
                else if(durationRequirementRange.Value == double.NegativeInfinity)
                {
                    if (maximumRetentionReached)
                    {
                        // Values up to the start value
                        IEnumerable<TimeSeriesTuple> samples = GetSamples(
                            sensor.ParameterName,
                            sensor.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM)
                            );
                        sensor.DurationStatus = SamplesMeetRequirement(samples, sensor.Requirement, true);
                    }
                    else
                        sensor.DurationStatus = DurationStatus.REQUIREMENT_MET;
                }
                else
                {
                    bool minimumRetentionReached = parameterTimeSeries.RetentionReached(retentionTime: sensor.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM));
                    if (maximumRetentionReached)
                    {
                        if (sensor.DurationStatus == DurationStatus.REQUIREMENT_MET)
                        {
                            sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
                        }
                        else
                        {
                            // Values up to the start value
                            IEnumerable<TimeSeriesTuple> upToValueSamples = GetSamples(
                                sensor.ParameterName,
                                sensor.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                                );
                            sensor.DurationStatus = SamplesMeetRequirement(upToValueSamples, sensor.Requirement, false);

                            // Up until value - has to be true
                            if (SamplesMeetRequirement(upToValueSamples, sensor.Requirement, false) == DurationStatus.REQUIREMENT_NOT_MET)
                                sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
                            else
                            {
                                // Values above the start value up to the end value
                                IEnumerable<TimeSeriesTuple> upToEndValueSamples = GetSamples(
                                    sensor.ParameterName,
                                    sensor.Duration.RetentionTime(requirementTime: RequirementTime.MAXIMUM),
                                    sensor.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                                    );

                                // Value to end value - mustn't all be true
                                sensor.DurationStatus = SamplesMeetRequirement(upToEndValueSamples, sensor.Requirement, true);
                            }
                        }
                    }
                    else if (minimumRetentionReached)
                    {
                        if (sensor.DurationStatus == DurationStatus.REQUIREMENT_MET)
                        {
                            sensor.DurationStatus = DurationStatusHelper.FromBool(sensor.Requirement.RequirementMet(latestSample.Val));
                        }
                        else
                        {
                            // Values up to the start value
                            IEnumerable<TimeSeriesTuple> samples = GetSamples(
                                sensor.ParameterName,
                                sensor.Duration.RetentionTime(requirementTime: RequirementTime.MINIMUM)
                                );
                            sensor.DurationStatus = SamplesMeetRequirement(samples, sensor.Requirement, false);
                        }
                    }
                    else
                        sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
                }
            }

            return sensor.DurationStatus;
        }

        private void SensorToTimeseries(SensorRequirement sensor)
        {
            _redisCaheService.CreateTimeSeries(
                    sensor.ParameterName.ToLower(),
                    sensor.Duration == null ? 1 : sensor.Duration.RetentionTime()
                    );
        }

        private void ReuploadToRedis(SensorRequirement sensor)
        {
            if (!_redisCaheService.ContainsKey(sensor.ParameterName.ToLower()))
                SensorToTimeseries(sensor);
        }
        
        private DurationStatus SamplesMeetRequirement(IEnumerable<TimeSeriesTuple> samples, RequirementParam requirement, bool reverseRequirement)
        {
            bool requirementBroken = samples.Any(sample => !requirement.RequirementMet(sample.Val));
            requirementBroken = reverseRequirement ? !requirementBroken : requirementBroken;
            return DurationStatusHelper.FromBool(!requirementBroken);
        }

        private void CacheParameter(string parameterName, TimeSeriesTuple sample)
        {
            _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName.ToLower()).Add(
                sample.Time, sample.Val
                );
        }

        private IEnumerable<TimeSeriesTuple> GetSamples(string parameterName, long retentionLength, long offsetTimestamp = 0)
        {
            var redisTimeSeries = _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName.ToLower());
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
