using LiveTelemetrySensor.SensorAlerts.Models;
using Microsoft.Extensions.Configuration;
using PdfExtractor.Models;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class LiveSensorFactory
    {
        private readonly string SOURCE_PATH;
        private IConfiguration _configuration;
        private AdditionalParser _additionalParser;
        public LiveSensorFactory(IConfiguration configuration, AdditionalParser additionalParser) 
        {
            SOURCE_PATH = Directory.GetCurrentDirectory().ToString();
            _configuration = configuration;
            _additionalParser = additionalParser;
        }
        private IEnumerable<SensorProperties> BuildSensorProperties()
        {
            string documentName = _configuration["SensorsProperties:DocumentName"];
            return TableProccessor.Instance.ProccessTable($"{SOURCE_PATH}/SensorAlerts/Documents/{documentName}.pdf");
        }

        private IEnumerable<SensorProperties> BuildTestProperties()
        {
            return new[]
            {
                new SensorProperties("Altitude", new RequirementRange(0, 45000), new RequirementRange(45000, 47000), new RequirementRange(47000, 50000), "When Longitude is lower than 120 for 3 seconds"),
                new SensorProperties("Longitude", new RequirementRange(-180, 170), new RequirementRange(170, 175), new RequirementRange(175, 180), "When Wind_Speed is higher than 176 for less than 3 seconds"),
                new SensorProperties("Wind_Speed", new RequirementRange(0, 55), new RequirementRange(55, 60), new RequirementRange(60, 70), "When Altitude is over 10000 for at least 3 seconds"),
                new SensorProperties("Engine_Heat", new RequirementRange(0, 70), new RequirementRange(70, 150), new RequirementRange(150, 300), "When Wind_Speed is over 176 for 3 to 5 seconds")
            };
        }

        public LiveSensor BuildLiveSensor(string sensorName, string additionalRequirement, IEnumerable<RequirementModel>? requirements = null)
        {

            return new LiveSensor(
                sensorName.ToLower(), 
                requirements == null ? Enumerable.Empty<RequirementModel>() : requirements,
                _additionalParser.Parse(additionalRequirement)
                );
        }

        public IEnumerable<LiveSensor> BuildLiveSensors()
        {
            foreach (var sensor in BuildTestProperties())
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
