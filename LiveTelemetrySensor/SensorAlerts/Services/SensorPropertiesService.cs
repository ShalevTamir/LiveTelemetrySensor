using LiveTelemetrySensor.SensorAlerts.Models;
using Microsoft.Extensions.Configuration;
using PdfExtractor.Models;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Services;
using System.Collections.Generic;
using System.IO;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorPropertiesService
    {
        private readonly string SOURCE_PATH;
        private IConfiguration _configuration;
        private AdditionalParser _additionalParser;
        public SensorPropertiesService(IConfiguration configuration, AdditionalParser additionalParser) 
        {
            SOURCE_PATH = Directory.GetCurrentDirectory().ToString();
            _configuration = configuration;
            _additionalParser = additionalParser;
        }
        private IEnumerable<SensorProperties> GenerateSensorProperties()
        {
            string documentName = _configuration["SensorsProperties:DocumentName"];
            return TableProccessor.Instance.ProccessTable($"{SOURCE_PATH}/SensorAlerts/Documents/{documentName}.pdf");
        }

        private IEnumerable<SensorProperties> GenerateTestProperties()
        {
            return new[]
            {
                new SensorProperties("Altitude", new RequirementRange(0, 45000), new RequirementRange(45000, 47000), new RequirementRange(47000, 50000), "When Longitude is lower than 120 for 3 minutes"),
                new SensorProperties("Longitude", new RequirementRange(-180, 170), new RequirementRange(170, 175), new RequirementRange(175, 180), "When Wind_Speed is higher than 176 for less than 3 minutes"),
                new SensorProperties("Wind_Speed", new RequirementRange(0, 55), new RequirementRange(55, 60), new RequirementRange(60, 70), "When Altitude is over 10000 for at least 3 minutes"),
                new SensorProperties("Engine_Heat", new RequirementRange(0, 70), new RequirementRange(70, 150), new RequirementRange(150, 300), "When Wind_Speed is over 176 for 3 to 5 minutes")
            };
        }


        public IEnumerable<LiveSensor> GenerateLiveSensors()
        {
            foreach (var sensor in GenerateTestProperties())
            {
                yield return new LiveSensor(
                    sensor.TelemetryParamName.ToLower(),
                    sensor.Requirements,
                    _additionalParser.Parse(sensor.AdditionalRequirement)
                    );
            }
        }

    }
}
