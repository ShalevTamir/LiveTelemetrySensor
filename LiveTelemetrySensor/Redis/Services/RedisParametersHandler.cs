using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using PdfExtractor.Models.Requirement;
using System.Collections;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisParametersHandler
    {
        //Includes a method to check if the requirement has been met for the duration - iterates over all cached values
        //Keeps track - if a duration requirement has been met - than only check current sensor requirement 

        //Key - parameter name, Value - the corresponding duration
        private Dictionary<string, Duration> _sensorRequirements;
        public RedisParametersHandler() { }

        //Stores all parameters with duration
        public void AddRelevantSensors(IEnumerable<LiveSensor> sensors)
        {
            foreach (var sensor in sensors)
            {
                foreach (SensorRequirement requirement in sensor.AdditionalRequirements)
                {
                    InsertDuration(requirement);
                }
            }
        }

        //Upon receiving a telmetry frame, cashes the relevant information
        public void ProcessTeleData(TelemetryFrameDto teleFrame)
        {
            foreach(var telemetryParameter in teleFrame.Parameters)
            {
                if (_sensorRequirements.ContainsKey(telemetryParameter.Name))
                {
                    //Cache information
                }
            }
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
