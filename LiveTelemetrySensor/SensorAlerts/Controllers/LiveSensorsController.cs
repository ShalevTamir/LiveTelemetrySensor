using Confluent.Kafka;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PdfExtractor.Models;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Services;
using SharpCompress.Common;
using Spire.Additions.Xps.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("live-sensors")]
    [ApiController]
    public class LiveSensorsController: Controller
    {
        private AdditionalParser _additionalParser;
        private TeleProcessorService _teleProcessor;
        private SensorsContainer _sensorsContainer;
        private RequestsService _requestsService;
        private LiveSensorFactory _liveSensorFactory;
        private const string LIVE_DATA_URL = "https://localhost:5003";

        public LiveSensorsController(
            TeleProcessorService teleProcessor,
            AdditionalParser additionalParser,
            SensorsContainer sensorsContainer,
            RequestsService requestsService,
            LiveSensorFactory liveSensorFactory)
        {
            _additionalParser = additionalParser;
            _teleProcessor = teleProcessor;
            _sensorsContainer = sensorsContainer;
            _requestsService = requestsService;
            _liveSensorFactory = liveSensorFactory;
        }

        [HttpGet("parse-sensor")]
        public async Task<IActionResult> ParseSensor(string sensorName, string sensorRequirements)
        {
            try
            {
                //return status code instead of exceptions
                EnsureNoDuplicateSensor(sensorName);
                var parsedSensorRequirements = await _additionalParser.Parse(sensorRequirements);
                await EnsureNoUnkownParametersAsync(parsedSensorRequirements);
                ValidateSensorRequirements(parsedSensorRequirements);
                return Ok(JsonConvert.SerializeObject(
                    parsedSensorRequirements.Select((sensorReuirement) => sensorReuirement.ToRequirementDto())));
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("sensor-requirements")]
        public IActionResult GetSensorRequirements(string sensorName)
        {
            if (_sensorsContainer.hasSensor(sensorName))
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new IgnorePropertiesContractResolver(new string[] { "SensedParamName" })
                };
                return Ok(JsonConvert.SerializeObject(
                    _sensorsContainer.GetSensor(sensorName), settings)
                    );
            }
            else
            {
                return BadRequest("Sensor " + sensorName + " doesn't exist");
            }
        }

        [HttpPost("add-parameter-sensors")]
        public IActionResult AddParameterSensors([FromBody] ParameterSensorDto[] parameterSensors)
        {
            try
            {
                _teleProcessor.AddSensorsToUpdate(parameterSensors.Select(parameterSensorDto => parameterSensorDto.ToLiveSensor()).ToArray());

            }catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPost("add-dynamic-sensor")]
        public ActionResult AddDynamicSensor([FromBody] DynamicSensorDto dynamicSensorDto)
        {
            try
            {
                //TODO: Parse requirement directly with defaultValueHandling instead of converting to requirement
                _teleProcessor.AddSensorToUpdate(new DynamicLiveSensor(
                    dynamicSensorDto.SensorName,
                    dynamicSensorDto.Requirements.Select((requirmentDto) => requirmentDto.ToSensorRequirement()).ToArray()
                    ));
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPost("remove-sensor")]
        public ActionResult RemoveSensor([FromBody] string sensorName)
        {
            try
            {
                _teleProcessor.RemoveSensorToUpdate(sensorName);
                return Ok();
            }catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("sensors-requirements")]
        public async Task<ActionResult> ParseParameterSensors([FromForm] IFormFile file)
        {
            IEnumerable<SensorProperties> parsedProperties = TableProccessor.Instance.ProccessTable(file.OpenReadStream());
            var sensors = await _liveSensorFactory.BuildLiveSensorsAsync(parsedProperties);
            return Ok(JsonConvert.SerializeObject(sensors.Select(parameterSensor => parameterSensor.ToParameterSensorDto()).ToArray()));
        }

        private void ValidateSensorRequirements(SensorRequirement[] parsedSensorRequirements)
        {
            IEnumerable<string> invalidRangeParameters = GetInvalidRangeParameters(parsedSensorRequirements);

            if (invalidRangeParameters.Count() != 0)
            {
                throw new ArgumentException(string.Format("Invalid range for {0} {1}",
                    invalidRangeParameters.Count() == 1 ? "parameter" : "parameters",
                    string.Join(", ", invalidRangeParameters)));
            }
            if (parsedSensorRequirements.Length == 0)
            {
                throw new ArgumentException("No sensor requirements detected");
            }
        }

        private async Task EnsureNoUnkownParametersAsync(SensorRequirement[] parsedSensorRequirements)
        {
            var parameterNames = await _requestsService.GetAsync<string[]>(LIVE_DATA_URL + "/parameters-config/parameter-names");
            IEnumerable<string> unkownParameterNames = GetUnkownParameterNames(parsedSensorRequirements, parameterNames);

            if (unkownParameterNames.Count() != 0)
            {
                throw new ArgumentException(string.Format("{0} {1} {2} not recognized",
                    unkownParameterNames.Count() == 1 ? "Parameter" : "Parameters",
                    string.Join(", ", unkownParameterNames),
                    unkownParameterNames.Count() == 1 ? "is" : "are"));
            }
        }

        private void EnsureNoDuplicateSensor(string sensorName)
        {
            if (_sensorsContainer.hasSensor(sensorName))
            {
                throw new ArgumentException("Sensor with name " + sensorName + " already exists");
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
                .Select((unkownSensorRequirement) =>  unkownSensorRequirement.ParameterName);
        }
    }
}
