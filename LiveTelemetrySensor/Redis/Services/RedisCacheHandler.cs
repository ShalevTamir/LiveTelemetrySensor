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
        public void AddRelevantSensors(IEnumerable<LiveSensor> sensors)
        {
            foreach (var sensor in sensors.Where(sensor => sensor.AdditionalRequirements != null))
            {
                foreach (SensorRequirement requirement in sensor.AdditionalRequirements)
                {
                    if (!_redisCaheService.ContainsKey(requirement.ParameterName.ToLower()))
                    {
                        _redisCaheService.CreateTimeSeries(
                            requirement.ParameterName.ToLower(),
                            requirement.Duration.RetentionTime()
                            );
                    }
                }
            }
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach (var telemetryParameter in
                teleFrame.Parameters.Where(param => _redisCaheService.ContainsKey(param.Name.ToLower())))
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
            if (!_redisCaheService.ContainsKey(sensor.ParameterName.ToLower()))
            {
                ReuploadToRedis(sensor);
                _redisCaheService.CreateTimeSeries(
                    sensor.ParameterName.ToLower(),
                    sensor.Duration.RetentionTime()
                    );
            }

            // TODO: check if user enter 0 minutes, could be problamatic
            if (_redisCaheService.GetStoredObject<RedisTimeSeries>(sensor.ParameterName.ToLower()).RetentionReached())
            {
                sensor.DurationStatus = CheckCachedData(sensor);
            }
            else
            {
                sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
            }
            return sensor.DurationStatus;
        }

        private void ReuploadToRedis(SensorRequirement sensor)
        {
            if (!_redisCaheService.ContainsKey(sensor.ParameterName.ToLower()))
                _redisCaheService.CreateTimeSeries(
                    sensor.ParameterName.ToLower(),
                    sensor.Duration.RetentionTime()
                    );
        }

        private DurationStatus CheckCachedData(SensorRequirement sensor)
        {
            TimeSeriesTuple? latestSample = _redisCaheService.GetStoredObject<RedisTimeSeries>(sensor.ParameterName.ToLower()).GetLastestSample();
            Debug.Assert(latestSample != null);
            IEnumerable<TimeSeriesTuple> samples = GetSamples(sensor.ParameterName, sensor.Duration.RetentionTime(), latestSample);
            if (sensor.Duration.RequirementParam is RequirementRange durationRequirementRange)
            {
                if (durationRequirementRange.Value == double.NegativeInfinity || durationRequirementRange.EndValue == double.PositiveInfinity)
                {
                    return SamplesMeetRequirement(samples, sensor.Requirement, durationRequirementRange.Value == double.NegativeInfinity);
                }
                else
                {
                    // Values up to the start value
                    IEnumerable<TimeSeriesTuple> upToValueSamples = 
                        samples.Where(sample => sample.Time >= latestSample.Time - durationRequirementRange.Value);

                    // up until value - has to be true
                    if (SamplesMeetRequirement(upToValueSamples, sensor.Requirement, false) == DurationStatus.REQUIREMENT_NOT_MET)
                        return DurationStatus.REQUIREMENT_NOT_MET;


                    // Values above the start value up to the end value
                    IEnumerable<TimeSeriesTuple> upToEndValueSamples =
                        samples.Where(sample => sample.Time <= latestSample.Time - durationRequirementRange.Value);

                    // value to end value - can't all be true
                    return SamplesMeetRequirement(upToEndValueSamples, sensor.Requirement, true);
                }

            }
            else
            {
                return SamplesMeetRequirement(samples, sensor.Requirement, false);
            }

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

        private IEnumerable<TimeSeriesTuple> GetSamples(string parameterName, long retentionLength, TimeSeriesTuple currentSample)
        {

            var earlySamples = _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName.ToLower()).GetReverseRange(
                RedisTimeSeries.REDIS_EARLIEST_SAMPLE,
                currentSample.Time - retentionLength
                );

            long beforeTimeframe;
            try
            {
                TimeSeriesTuple latestBeforeRetention = earlySamples.First(sample => sample.Time < currentSample.Time - retentionLength);
                beforeTimeframe = latestBeforeRetention.Time;

            }
            catch(InvalidOperationException)
            {
                // Shouldn't get here if RetentionReached is true
                beforeTimeframe = currentSample.Time - retentionLength;
            }
            var samplesToReturn = new List<TimeSeriesTuple>();
            samplesToReturn.AddRange(
                _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName.ToLower()).GetRange(
                    beforeTimeframe,
                    RedisTimeSeries.REDIS_LATEST_SAMPLE
                    )
                );
            samplesToReturn.Add(currentSample);

            // If RetentionReached is true then the total time length of the samples should be bigger than retention length after insertion of latestDeletedSample
            TimeSeriesTuple? latestDeletedSample = _redisCaheService.GetStoredObject<RedisTimeSeries>(parameterName.ToLower()).LatestDeletedSample;
            if (samplesToReturn.Count >= 2 &&
                samplesToReturn[samplesToReturn.Count - 1].Time - samplesToReturn[0].Time < retentionLength &&
                latestDeletedSample != null)
                samplesToReturn.Insert(0, latestDeletedSample);

            return samplesToReturn;
           
        }

        
    }
}
