using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
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
            return new SensorProperties[]
            {
                new SensorProperties("Altitude", new RequirementRange(0, 10000), new RequirementRange(10000, 20000), new RequirementRange(20000, 50000), "When Longitude is lower than 120 for 1 to 2 minutes"),
                new SensorProperties("Longitude", new RequirementRange(-180, 170), new RequirementRange(170, 175), new RequirementRange(175, 180), "When Wind_Speed is higher than 176 for less than 3 seconds"),
                new SensorProperties("Wind_Speed", new RequirementRange(0, 55), new RequirementRange(55, 60), new RequirementRange(60, 70), "When Altitude is over 10000 for at least 3 seconds"),
                new SensorProperties("Engine_Heat", new RequirementRange(0, 70), new RequirementRange(70, 150), new RequirementRange(1500000, 1500001), "When Wind_Speed is over 176 for 3 to 5 seconds")
            };
        }

        public BaseSensor BuildLiveSensor(string sensorName, string additionalRequirement, IEnumerable<RequirementModel>? requirements = null)
        {
            if (requirements == null)
                return new DynamicLiveSensor(sensorName, _additionalParser.Parse(additionalRequirement));
            else
                return new ParameterLiveSensor(sensorName, _additionalParser.Parse(additionalRequirement), requirements);
         }

        public IEnumerable<BaseSensor> BuildLiveSensors()
        {
            foreach (var sensor in BuildTestProperties())
            {
                yield return BuildLiveSensor(sensor.TelemetryParamName, sensor.AdditionalRequirement, sensor.Requirements);
            }
        }

    }
}
