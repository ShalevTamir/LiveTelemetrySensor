using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("live-sensor-alerts")]
    [ApiController]
    public class LiveSensorAlertsController : Controller
    {
        private SensorAlertsService _sensorAlerts;
        public LiveSensorAlertsController(SensorAlertsService sensorAlertsService) 
        {
            _sensorAlerts = sensorAlertsService;
        }

        [HttpGet]
        public ActionResult GetSensorsState()
        {
            return Ok(JsonConvert.SerializeObject(_sensorAlerts.GetSensorsState().ToArray()));
        }

        [HttpPut("state")]
        public ActionResult ChangeSensorState([FromBody] RunningState stateToChangeTo)
        {
            bool success = _sensorAlerts.ChangeState(stateToChangeTo);
            if (success) return Ok();
            else return BadRequest("Server is already in state " + stateToChangeTo);
        }

        [HttpPost("add-sensor")]
        public ActionResult AddDirectSensor([FromBody] DirectSensorDto directSensorDto)
        {
            DynamicLiveSensor sensor;
            try
            {
                sensor = _sensorAlerts.AddDirectSensor(directSensorDto.SensorName, directSensorDto.AdditionalRequirements);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(JsonConvert.SerializeObject(
                sensor.AdditionalRequirements.Select((SensorRequirement sensorRequirement) =>
                {
                    return new SensorRequirementDto
                    {
                        ParameterName = sensorRequirement.ParameterName,
                        Requirement = sensorRequirement.Requirement,
                        Duration = sensorRequirement.Duration,
                    };
                }
            )));
        }

   
    }
}
