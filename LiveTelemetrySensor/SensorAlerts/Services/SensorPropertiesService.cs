using LiveTelemetrySensor.SensorAlerts.Models;
using Microsoft.Extensions.Configuration;
using PdfExtractor.Models;
using PdfExtractor.Services;
using System.Collections.Generic;
using System.IO;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorPropertiesService
    {
        private readonly string SOURCE_PATH;
        private IConfiguration _configuration;
        public SensorPropertiesService(IConfiguration configuration) 
        {
            SOURCE_PATH = Directory.GetCurrentDirectory().ToString();
            _configuration = configuration;
        }
        private IEnumerable<SensorProperties> GenerateSensorProperties()
        {
            string documentName = _configuration["SensorsProperties:DocumentName"];
            return TableProccessor.Instance.ProccessTable($"{SOURCE_PATH}/SensorAlerts/Documents/{documentName}.pdf");
        }

        public IEnumerable<LiveSensor> GenerateLiveSensors()
        {
            foreach (var sensor in GenerateSensorProperties())
            {
                yield return new LiveSensor(sensor.TelemetryParamName,sensor.Requirements,sensor.AdditionalRequirement);
            }
        }

    }
}
