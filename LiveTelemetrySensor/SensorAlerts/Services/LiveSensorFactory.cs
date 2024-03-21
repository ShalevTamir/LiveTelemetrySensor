using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using Microsoft.Extensions.Configuration;
using PdfExtractor.Models;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private IEnumerable<SensorProperties> BuildDefaultSensorProperties()
        {
            string documentName = _configuration["SensorsProperties:DocumentName"];
            return TableProccessor.Instance.ProccessTable($"{SOURCE_PATH}/SensorAlerts/Documents/{documentName}.pdf");
        }

        private IEnumerable<SensorProperties> BuildTestProperties()
        {
            return new SensorProperties[]
            {
                //new SensorProperties("Altitude", new RequirementRange(0, 10000), new RequirementRange(10000, 20000), new RequirementRange(20000, 50000), "When Longitude is lower than 120 for 1 to 2 minutes"),
                //new SensorProperties("Longitude", new RequirementRange(-180, 170), new RequirementRange(170, 175), new RequirementRange(175, 180), "When Wind_Speed is higher than 176 for less than 3 seconds"),
                //new SensorProperties("Wind_Speed", new RequirementRange(0, 55), new RequirementRange(55, 60), new RequirementRange(60, 70), "When Altitude is over 10000 for at least 3 seconds"),
                new SensorProperties("Engine_Heat", new RequirementRange(0, 176), new RequirementRange(73230, 132350), new RequirementRange(0, 300), "When engine_heat is over 176 for 10 seconds"),
            };
        }

        public async Task<BaseSensor> BuildLiveSensor(string sensorName, string additionalRequirement, IEnumerable<RequirementModel>? requirements = null)
        {
            if (requirements == null)
                return new DynamicLiveSensor(sensorName, await _additionalParser.Parse(additionalRequirement));
            else
                return new ParameterLiveSensor(sensorName, await _additionalParser.Parse(additionalRequirement), requirements);
         }

        public async Task<ParameterLiveSensor[]> BuildDefaultParameterSensorsAsync()
        {
            var sensorProperties = BuildDefaultSensorProperties();
            return await BuildParameterSensorsAsync(sensorProperties);
        }


        public async Task<ParameterLiveSensor[]> BuildParameterSensorsAsync(IEnumerable<SensorProperties> sensorProperties)
        {
            var sensorsTasks = sensorProperties.Select(
                async (sensor) => (ParameterLiveSensor) await BuildLiveSensor(
                    sensor.TelemetryParamName,
                    sensor.AdditionalRequirement,
                    sensor.Requirements
                ));
            return await Task.WhenAll(sensorsTasks);
        }

    }
}
