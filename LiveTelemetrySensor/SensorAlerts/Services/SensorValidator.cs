using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorValidator
    {
        private const string LIVE_DATA_URL = "https://localhost:5003";

        private SensorsContainer _sensorsContainer;
        private RequestsService _requestsService;

        public SensorValidator(
            SensorsContainer sensorsContainer,
            RequestsService requestsService) 
        {
            _sensorsContainer = sensorsContainer;
            _requestsService = requestsService;
        }
        public SensorValidationResult CheckSensorExists(string sensorName)
        {
            if (_sensorsContainer.hasSensor(sensorName))
            {
                return new ValidSensorResult();
            }
            else
            {
                return new DoesntExistResult(sensorName);
            }
            
        }
        public SensorValidationResult SensorNameValidation(string sensorName)
        {
            return CheckDuplicateSensor(sensorName);
        }

        public async Task<SensorValidationResult> SensorRequirementsValidationAsync(SensorRequirement[] requirements)
        {
            return await CheckSensorRequirementsAsync(requirements);
        }

        private async Task<SensorValidationResult> CheckSensorRequirementsAsync(SensorRequirement[] parsedSensorRequirements)
        {
            IEnumerable<string> invalidRangeParameters = GetInvalidRangeParameters(parsedSensorRequirements);

            if (invalidRangeParameters.Count() != 0)
            {
                return new InvalidRangeResult(invalidRangeParameters);
            }
            if (parsedSensorRequirements.Length == 0)
            {
                return new EmptyRequirementsResult("No sensor requirements detected");
            }
            return await CheckUnkownParametersAsync(parsedSensorRequirements);
        }

        private async Task<SensorValidationResult> CheckUnkownParametersAsync(SensorRequirement[] parsedSensorRequirements)
        {
            var parameterNames = await _requestsService.GetAsync<string[]>(LIVE_DATA_URL + "/parameters-config/parameter-names");
            IEnumerable<string> unkownParameterNames = GetUnkownParameterNames(parsedSensorRequirements, parameterNames);
            if (unkownParameterNames.Count() != 0)
            {
                return new UnkownParametersResult(unkownParameterNames);
            }
            else
            {
                return new ValidSensorResult();
            }
        }

        private SensorValidationResult CheckDuplicateSensor(string sensorName)
        {
            if (_sensorsContainer.hasSensor(sensorName))
            {
                return new DuplicateSensorResult(sensorName);
            }
            else
            {
                return new ValidSensorResult();
            }
        }

        private IEnumerable<string> GetInvalidRangeParameters(SensorRequirement[] sensorRequirements)
        {
            return sensorRequirements
                .Where((sensorRequirement) => !sensorRequirement.IsValid())
                .Select((invalidSensorRequirement) => invalidSensorRequirement.ParameterName);
        }

        private IEnumerable<string> GetUnkownParameterNames(SensorRequirement[] sensorRequirements, string[] parameterNames)
        {
            return sensorRequirements.Where((sensorRequirement) =>
            !parameterNames.Any((parameterName) => sensorRequirement.ParameterName == parameterName.ToLower()))
                .Select((unkownSensorRequirement) => unkownSensorRequirement.ParameterName);
        }

    }
}
