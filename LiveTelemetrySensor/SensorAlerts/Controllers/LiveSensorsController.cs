using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PdfExtractor.Models.Requirement;
using Spire.Additions.Xps.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("live-sensors")]
    [ApiController]
    public class LiveSensorsController: Controller
    {
        private AdditionalParser _additionalParser;
        private TeleProcessorService _teleProcessor;
        public LiveSensorsController(TeleProcessorService teleProcessor, AdditionalParser additionalParser)
        {
            _additionalParser = additionalParser;
            _teleProcessor = teleProcessor;
        }

        [HttpGet("has-sensor")]
        public ActionResult HasSensor(string sensorName)
        {
            return Ok(_teleProcessor.HasSensor(sensorName));
        }

        [HttpGet("parse-sensor")]
        public async Task<ActionResult> ParseSensor(string sensorRequirements)
        {
            var parsedSensorRequirements = await _additionalParser.Parse(sensorRequirements);
            IEnumerable<string> invalidRangeParameters = parsedSensorRequirements
                .Where((sensorRequirement) =>
                (sensorRequirement.Requirement is RequirementRange range && !range.IsValidRange()) ||
                (sensorRequirement.Duration != null && sensorRequirement.Duration.Requirement is RequirementRange durationRange && !durationRange.IsValidRange()))
                .Select((invalidSensorRequirement) => invalidSensorRequirement.ParameterName);

            if(invalidRangeParameters.Count() != 0)
            {
                return BadRequest(string.Format("Invalid range for {0} {1}",
                    invalidRangeParameters.Count() == 1 ? "parameter": "parameters",
                    string.Join(", ",invalidRangeParameters)));
            }

            return Ok(JsonConvert.SerializeObject(
                parsedSensorRequirements.Select((SensorRequirement sensorRequirement) =>
                {
                    var sensorRequirementDto = new SensorRequirementDto
                    {
                        ParameterName = sensorRequirement.ParameterName,
                        Requirement = new RequirementDto(sensorRequirement.Requirement),
                        
                    };
                    if(sensorRequirement.Duration != null)
                    {
                        sensorRequirementDto.Duration = new DurationDto(sensorRequirement.Duration);
                    }
                    return sensorRequirementDto;
                }
            )));

        }

        [HttpPost("add-sensor")]
        public ActionResult AddDynamicSensor([FromBody] DynamicSensorDto dynamicSensorDto)
        {
            try
            {
                _teleProcessor.AddSensorToUpdate(
                    new DynamicLiveSensor(
                        dynamicSensorDto.SensorName,
                        dynamicSensorDto.Requirements.Select((requirementDto) => new SensorRequirement(
                            requirementDto.ParameterName,
                            requirementDto.Requirement.ToRequirementParam(),
                            requirementDto.Duration?.ToDuration()
                            )).ToArray()
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
    }
}
