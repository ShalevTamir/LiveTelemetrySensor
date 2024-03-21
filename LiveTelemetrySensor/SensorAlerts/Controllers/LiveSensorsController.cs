using Confluent.Kafka;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [Route("live-sensors")]
    [ApiController]
    public class LiveSensorsController: Controller
    {
        private AdditionalParser _additionalParser;
        private TeleProcessorService _teleProcessor;
        private SensorsContainer _sensorsContainer;
        private LiveSensorFactory _liveSensorFactory;
        private SensorValidator _sensorValidator;
        

        public LiveSensorsController(
            TeleProcessorService teleProcessor,
            AdditionalParser additionalParser,
            SensorsContainer sensorsContainer,
            LiveSensorFactory liveSensorFactory,
            SensorValidator sensorValidator)
        {
            _additionalParser = additionalParser;
            _teleProcessor = teleProcessor;
            _sensorsContainer = sensorsContainer;
            _liveSensorFactory = liveSensorFactory;
            _sensorValidator = sensorValidator;
        }

        [HttpGet("parse-sensor")]
        public async Task<IActionResult> ParseSensor(string sensorName, string sensorRequirements)
        {
            var validation = _sensorValidator.SensorNameValidation(sensorName);
            if(validation.IsValid())
            {
                var parsedSensorRequirements = await _additionalParser.Parse(sensorRequirements);
                validation = await _sensorValidator.SensorRequirementsValidationAsync(parsedSensorRequirements);
                validation.SetValidMessage(JsonConvert.SerializeObject(
                    parsedSensorRequirements.Select((sensorReuirement) => sensorReuirement.ToRequirementDto())));
            }

            return validation.ToIActionResult();
        }

        [HttpGet("sensor-requirements")]
        public IActionResult GetSensorRequirements(string sensorName)
        {
            var validation = _sensorValidator.CheckSensorExists(sensorName);
            if (validation.IsValid())
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new IgnorePropertiesContractResolver(new string[] { "SensedParamName" })
                };
                validation.SetValidMessage(JsonConvert.SerializeObject(
                    _sensorsContainer.GetSensor(sensorName), settings));
            }
            return validation.ToIActionResult();
        }

        [HttpPost("add-parameter-sensors")]
        public IActionResult AddParameterSensors([FromBody] ParameterSensorDto[] parameterSensors)
        {
            var validation = _teleProcessor.AddSensorsToUpdate(parameterSensors.Select(parameterSensorDto => parameterSensorDto.ToLiveSensor()).ToArray());
            return validation.ToIActionResult();
        }

        [HttpPost("add-dynamic-sensor")]
        public IActionResult AddDynamicSensor([FromBody] DynamicSensorDto dynamicSensorDto)
        {
                //TODO: Parse requirement directly with defaultValueHandling instead of converting to requirement
            var sensorResult = _teleProcessor.AddSensorToUpdate(new DynamicLiveSensor(
                dynamicSensorDto.SensorName,
                dynamicSensorDto.Requirements.Select((requirmentDto) => requirmentDto.ToSensorRequirement()).ToArray()
                ));
            return sensorResult.ToIActionResult();
        }

        [HttpPost("remove-sensor")]
        public IActionResult RemoveSensor([FromBody] string sensorName)
        {
            var validation = _teleProcessor.RemoveSensorToUpdate(sensorName);
            return validation.ToIActionResult();
        }

        [HttpPost("sensors-requirements")]
        public async Task<IActionResult> ParseParameterSensors([FromForm] List<IFormFile> files)
        {
            ParameterLiveSensor[] sensors;
            try
            {
                IEnumerable<SensorProperties> parsedProperties = files.SelectMany(file => TableProccessor.Instance.ProccessTable(file.FileName, file.OpenReadStream()));
                sensors = await _liveSensorFactory.BuildParameterSensorsAsync(parsedProperties);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(JsonConvert.SerializeObject(sensors.Select(parameterSensor => parameterSensor.ToParameterSensorDto()).ToArray()));
        }

        
    }
}
