using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using NRedisStack.DataTypes;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisCacheHandler
    {

        //Key - parameter name, Value - the corresponding duration
        private Dictionary<string, Duration> _sensorsDurations;
        private const long RETENTION_MARGIN_IN_MILLIS = 2000;

        private RedisCacheService _redisCaheService;
        public RedisCacheHandler(RedisCacheService cacheService)
        {
            _sensorsDurations = new Dictionary<string, Duration>();
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
                    InsertDuration(requirement);
                    if (!_redisCaheService.ContainsKey(requirement.ParameterName.ToLower()))
                    {
                        string paramName = requirement.ParameterName.ToLower();
                        _redisCaheService.TimeSeriesHandler.Create(
                            paramName,
                            _sensorsDurations[paramName].RetentionTime() + RETENTION_MARGIN_IN_MILLIS
                            );
                    }
                }
            }
        }

        //Receives a telmetry frame, cashes the relevant information
        public void CacheTeleData(TelemetryFrameDto teleFrame)
        {
            foreach (var telemetryParameter in 
                teleFrame.Parameters.Where(param => _sensorsDurations.ContainsKey(param.Name.ToLower())))
            {
                CacheParameter(
                    telemetryParameter,
                    _sensorsDurations[telemetryParameter.Name.ToLower()],
                    teleFrame.TimeStamp
                    );
            }
        }

        //Checks if the requirement has been met for the duration - iterates over all cached values
        //if a duration requirement already met - only checks current latest param value to match requirement
        public DurationStatus UpdateDurationStatus(SensorRequirement sensor)
        {
            if (_sensorsDurations.ContainsKey(sensor.ParameterName.ToLower()))
            {
                var latestSample = _redisCaheService.TimeSeriesHandler.GetLastSample(sensor.ParameterName.ToLower());
                var durationLengthInEpoc = sensor.Duration.RetentionTime();
                if (sensor.DurationStatus == DurationStatus.REQUIREMENT_MET)
                {
                    double latestParamValue = latestSample.Val;
                    sensor.DurationStatus = (DurationStatus)Convert.ToInt32(sensor.Requirement.RequirementMet(latestParamValue));
                }
                else
                {
                    // if user enter 0 minutes, could be problamatic
                    if (!durationLengthInEpoc.HasValue ||
                        _redisCaheService.TimeSeriesHandler.DurationReached(sensor.ParameterName.ToLower(), (long)durationLengthInEpoc))
                    {
                        sensor.DurationStatus = CheckCachedData(sensor, latestSample, durationLengthInEpoc);
                    }
                    else
                    {
                        sensor.DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
                    }
                }
            }
            else
            {
                ReuploadToRedis(sensor.ParameterName.ToLower());
            }
            return sensor.DurationStatus;
        }

        private void ReuploadToRedis(string parameterName)
        {
            if (!_redisCaheService.ContainsKey(parameterName))
                _redisCaheService.TimeSeriesHandler.Create(parameterName);
        }

        private DurationStatus CheckCachedData(SensorRequirement sensor, TimeSeriesTuple latestSample, long? durationLengthInEpoc)
        {
            IReadOnlyList<TimeSeriesTuple> samples;
            if (durationLengthInEpoc.HasValue)
            {
                long from = latestSample.Time - (long)durationLengthInEpoc;
                long to = latestSample.Time;
                samples = _redisCaheService.TimeSeriesHandler.GetRange(sensor.ParameterName.ToLower(), from, to);
            }
            else
            {
                samples = _redisCaheService.TimeSeriesHandler.GetAllSamples(sensor.ParameterName.ToLower());
            }

            foreach (TimeSeriesTuple sample in samples)
            {
                if (!sensor.Requirement.RequirementMet(sample.Val))
                    return DurationStatus.REQUIREMENT_NOT_MET;
            }
            return DurationStatus.REQUIREMENT_MET;
        }

        private void CacheParameter(TelemetryParameterDto telemetryParameter, Duration duration, DateTime timestamp)
        {
            //long? retentionLength = duration.RetentionTime();
            //if (retentionLength.HasValue)
            //{
            //    TimeSeriesInformation info = _redisCaheService.TimeSeriesHandler.Info(telemetryParameter.Name.ToLower());
            //    //If maximum retention has reached, delete the oldest sample
            //    if (info.LastTimeStamp - info.FirstTimeStamp >= retentionLength)
            //    {
            //        _redisCaheService.TimeSeriesHandler.DeleteRange(
            //            telemetryParameter.Name.ToLower(), info.FirstTimeStamp, info.FirstTimeStamp
            //            );
            //    }
            //}
            _redisCaheService.TimeSeriesHandler.Add(
                telemetryParameter.Name.ToLower(), timestamp, double.Parse(telemetryParameter.Value)
                );
        }


        private void InsertDuration(SensorRequirement sensorRequirement)
        {
            if (!_sensorsDurations.ContainsKey(sensorRequirement.ParameterName))
                _sensorsDurations.Add(sensorRequirement.ParameterName, sensorRequirement.Duration);
            else
            {
                RequirementParam currentDurationLength = _sensorsDurations[sensorRequirement.ParameterName].RequirementParam;
                RequirementParam updatedDurationLength = sensorRequirement.Duration.RequirementParam;
                if (currentDurationLength.Compare(updatedDurationLength) == -1)
                    _sensorsDurations[sensorRequirement.ParameterName] = sensorRequirement.Duration;
            }
        }
    }
}
