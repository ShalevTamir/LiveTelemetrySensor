using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using PdfExtractor.Models.Requirement;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Redis.Services
{
    public class SensorsDurationHandler
    {
        //Key - parameter name, Value - the corresponding duration
        private Dictionary<string, Duration> _sensorsDurations;

        public SensorsDurationHandler()
        {
            _sensorsDurations = new Dictionary<string, Duration>(); 
        }

        public Duration GetDuration(string sensorName)
        {
            return _sensorsDurations[sensorName.ToLower()];
        }

        public bool ContainsParameter(string sensorName)
        {
            return _sensorsDurations.ContainsKey(sensorName.ToLower());
        }

        public void InsertDuration(SensorRequirement sensorRequirement)
        {
            InsertDuration(sensorRequirement.ParameterName, sensorRequirement.Duration);
        }

        public void InsertDuration(string sensorName, Duration duration)
        {
            if (!_sensorsDurations.ContainsKey(sensorName))
                _sensorsDurations.Add(sensorName, duration);
            else
            {
                RequirementParam currentDurationLength = _sensorsDurations[sensorName].RequirementParam;
                RequirementParam updatedDurationLength = duration.RequirementParam;
                if (currentDurationLength.Compare(updatedDurationLength) == -1)
                    _sensorsDurations[sensorName] = duration;
            }
        }
    }
}
