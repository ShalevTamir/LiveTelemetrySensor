using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SensorAlertsController : Controller
    {
        private SensorAlertsService _sensorAlerts;
        public SensorAlertsController(SensorAlertsService sensorAlertsService) 
        {
            _sensorAlerts = sensorAlertsService;
        }
        [HttpPut("state")]
        public ActionResult ChangeSensorState([FromBody] RunningState stateToChangeTo)
        {
            bool success = _sensorAlerts.ChangeState(stateToChangeTo);
            if (success) return Ok();
            else return BadRequest("Server is already in state " + stateToChangeTo);
        }

        [HttpPost("add-sensor")]
        public ActionResult ChangeSensorState([FromBody] DirectSensorDto directSensorDto)
        {
            try
            {
                _sensorAlerts.AddDirectSensor(directSensorDto.SensorName, directSensorDto.AdditionalRequirements);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok();
        }
    }
}
