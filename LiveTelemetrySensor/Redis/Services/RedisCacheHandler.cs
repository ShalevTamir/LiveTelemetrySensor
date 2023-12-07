using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using NRedisStack.DataTypes;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisCacheHandler
    {

        //Key - parameter name, Value - the corresponding duration
        private Dictionary<string, Duration> _sensorRequirements;

        private RedisCacheService _redisCaheService;
        public RedisCacheHandler(RedisCacheService cacheService) 
        {
            _redisCaheService = cacheService;
        }

        //Stores all parameters with duration
        public void AddRelevantSensors(IEnumerable<LiveSensor> sensors)
        {
            foreach (var sensor in sensors)
            {
                foreach (SensorRequirement requirement in sensor.AdditionalRequirements)
                {
                    InsertDuration(requirement);
                    _redisCaheService.TimeSeriesHandler.Create(
                        requirement.ParameterName.ToLower()
                        );
                }
            }
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach(var telemetryParameter in teleFrame.Parameters)
            {
                if (_sensorRequirements.ContainsKey(telemetryParameter.Name))
                {
                    CacheParameter(
                        telemetryParameter,
                        _sensorRequirements[telemetryParameter.Name.ToLower()],
                        teleFrame.TimeStamp
                        );
                }
            }
        }

        //Checks if the requirement has been met for the duration - iterates over all cached values
        //if a duration requirement already met - only checks current latest param value to match requirement
        public RequirementStatus DurrationRequirementStatus(string teleParamName)
        {
            if (!_sensorRequirements.ContainsKey(teleParamName.ToLower()))
                throw new ArgumentException(teleParamName + " isn't registered as a requirement having a duration");

            Duration paramDuration = _sensorRequirements[teleParamName.ToLower()];
            if (paramDuration.RequirementStatus == RequirementStatus.REQUIREMENT_MET)
            {
                double latestParamValue = _redisCaheService.TimeSeriesHandler.GetLastSample(teleParamName.ToLower()).Val;
                paramDuration.RequirementStatus = (RequirementStatus) Convert.ToInt32(paramDuration.RequirementParam.RequirementMet(latestParamValue));
            }
            else
            {
                paramDuration.RequirementStatus = UpdatedRequirementStatus(teleParamName.ToLower());
            }
            return paramDuration.RequirementStatus;
        }

        private RequirementStatus UpdatedRequirementStatus(string teleParamName)
        {
            IReadOnlyList<TimeSeriesTuple> samples = _redisCaheService.TimeSeriesHandler.GetAllSamples(teleParamName.ToLower());
            RequirementParam requirement = _sensorRequirements[teleParamName.ToLower()].RequirementParam;
            foreach (TimeSeriesTuple sample in samples)
            {
                if (!requirement.RequirementMet(sample.Val))
                    return RequirementStatus.REQUIREMENT_NOT_MET;
            }
            return RequirementStatus.REQUIREMENT_MET;
        }

        private void CacheParameter(TelemetryParameterDto telemetryParameter, Duration duration, DateTime timestamp)
        {
            long? retentionLength = DurationToRetention(duration);
            if (retentionLength.HasValue) 
            { 
                TimeSeriesInformation info = _redisCaheService.TimeSeriesHandler.Info(telemetryParameter.Name.ToLower());
                if (info.LastTimeStamp - info.FirstTimeStamp >= retentionLength)
                {
                    _redisCaheService.TimeSeriesHandler.DeleteRange(
                        telemetryParameter.Name.ToLower(), info.FirstTimeStamp, info.FirstTimeStamp
                        );
                }
            }
            _redisCaheService.TimeSeriesHandler.Add(
                telemetryParameter.Name.ToLower(), timestamp, double.Parse(telemetryParameter.Value)
                );
        }

        private long? DurationToRetention(Duration duration)
        {
            RequirementParam requirement = duration.RequirementParam;
            if (requirement is RequirementRange)
            {
                RequirementRange requirementRange = (RequirementRange)requirement;
                //For example: more than 5 minutes
                if (requirementRange.EndValue == double.PositiveInfinity)
                    return null;
                //For example: less than 5 minutes or between 3 to 5 minutes
                else 
                    return ConvertDurationValue(requirementRange.EndValue, duration.DurationType);
            }
            else
                return ConvertDurationValue(requirement.Value, duration.DurationType);
        }

        private long ConvertDurationValue(double durationValue, DurationType durationType)
        {
            return (long)(durationValue * (int)durationType);
        }
 
        private void InsertDuration(SensorRequirement sensorRequirement)
        {
            if(!_sensorRequirements.ContainsKey(sensorRequirement.ParameterName))
                _sensorRequirements.Add(sensorRequirement.ParameterName, sensorRequirement.Duration);
            else
            {
                RequirementParam currentDurationLength = _sensorRequirements[sensorRequirement.ParameterName].RequirementParam;
                RequirementParam updatedDurationLength = sensorRequirement.Duration.RequirementParam;
                if (currentDurationLength.Value < updatedDurationLength.Value)
                    _sensorRequirements[sensorRequirement.ParameterName] = sensorRequirement.Duration;
            }
        }
    }
}
