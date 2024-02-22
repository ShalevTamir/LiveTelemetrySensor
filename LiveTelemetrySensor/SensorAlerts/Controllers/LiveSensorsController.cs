using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("live-sensors")]
    [ApiController]
    public class LiveSensorsController: Controller
    {
        private SensorAlertsService _sensorAlerts;
        private AdditionalParser _additionalParser;
        public LiveSensorsController(SensorAlertsService sensorAlerts, AdditionalParser additionalParser)
        {
            _additionalParser = additionalParser;
            _sensorAlerts = sensorAlerts;

        }

        [HttpGet("parse-sensor")]
        public async Task<ActionResult> ParseSensor(string sensorRequirements)
        {
            var parsedSensorRequirements = await _additionalParser.Parse(sensorRequirements);
            return Ok(JsonConvert.SerializeObject(
                parsedSensorRequirements.Select((SensorRequirement sensorRequirement) =>
                {
                    return new SensorRequirementDto
                    {
                        ParameterName = sensorRequirement.ParameterName,
                        Requirement = new RequirementDto(sensorRequirement.Requirement),
                        Duration = sensorRequirement.Duration,
                    };
                }
            )));

        }

        [HttpPost("add-sensor")]
        public ActionResult AddDynamicSensor([FromBody] DynamicSensorDto dynamicSensorDto)
        {
            try
            {
                _sensorAlerts.AddDynamicSensor(
                    new DynamicLiveSensor(
                        dynamicSensorDto.SensorName,
                        dynamicSensorDto.Requirements.Select((requirementDto) => new SensorRequirement(
                            requirementDto.ParameterName,
                            requirementDto.Requirement.ToRequirementParam(),
                            requirementDto.Duration
                            ))
                        ));
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
