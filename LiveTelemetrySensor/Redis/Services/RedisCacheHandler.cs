using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using NRedisStack.DataTypes;
using PdfExtractor.Models.Requirement;
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
        public SensorsDurationHandler _sensorsDurationHandler;
        public RedisCacheHandler(RedisCacheService cacheService)
        {
            _sensorsDurationHandler = new SensorsDurationHandler();
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
                    _sensorsDurationHandler.InsertDuration(requirement);
                    if (!_redisCaheService.ContainsKey(requirement.ParameterName.ToLower()))
                    {
                        _redisCaheService.TimeSeriesHandler.Create(
                            requirement.ParameterName.ToLower(),
                            _sensorsDurationHandler.GetDuration(requirement.ParameterName).RetentionTime()
                            );
                    }
                }
            }
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach (var telemetryParameter in
                teleFrame.Parameters.Where(param => _sensorsDurationHandler.ContainsParameter(param.Name)))
            {
                CacheParameter(
                    telemetryParameter.Name,
                    new TimeSeriesTuple(teleFrame.TimeStamp, double.Parse(telemetryParameter.Value))
                    );
            }
        }

        //Checks if the requirement has been met for the duration - iterates over all cached values
        //if a duration requirement already met - only checks current latest param value to match requirement

        private DurationStatus UpdateDurationStatus(SensorRequirement sensor)
        {
            if (!_sensorsDurationHandler.ContainsParameter(sensor.ParameterName))
            {
                ReuploadToRedis(sensor.ParameterName.ToLower());
                _sensorsDurationHandler.InsertDuration(sensor);
            }

            // TODO: check if user enter 0 minutes, could be problamatic
            if (_redisCaheService.TimeSeriesHandler.RetentionReached(sensor.ParameterName.ToLower()))
            {
                sensor.DurationStatus = CheckCachedData(sensor);
            }
            else
            {
                sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
            }
            return sensor.DurationStatus;
        }

        private void ReuploadToRedis(string parameterName)
        {
            if (!_redisCaheService.ContainsKey(parameterName))
                _redisCaheService.TimeSeriesHandler.Create(parameterName);
        }

        private DurationStatus CheckCachedData(SensorRequirement sensor)
        {
            TimeSeriesTuple latestSample = _redisCaheService.TimeSeriesHandler.GetLastestSample(sensor.ParameterName.ToLower());
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
            _redisCaheService.TimeSeriesHandler.Add(
                parameterName.ToLower(), sample.Time, sample.Val
                );
        }

        private IEnumerable<TimeSeriesTuple> GetSamples(string parameterName, long retentionLength, TimeSeriesTuple currentSample)
        {

            var earlySamples = _redisCaheService.TimeSeriesHandler.GetReverseRange(
                parameterName.ToLower(),
                new TimeStamp(TimeSeriesHandler.REDIS_EARLIEST_SAMPLE),
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
                beforeTimeframe = currentSample.Time - retentionLength;
            }
            var samplesToReturn = new List<TimeSeriesTuple>();
            samplesToReturn.AddRange(
                _redisCaheService.TimeSeriesHandler.GetRange(
                    parameterName.ToLower(),
                    beforeTimeframe,
                    new TimeStamp(TimeSeriesHandler.REDIS_LATEST_SAMPLE)
                    )
                );
            samplesToReturn.Add(currentSample);

            return samplesToReturn;
           
        }

        
    }
}
