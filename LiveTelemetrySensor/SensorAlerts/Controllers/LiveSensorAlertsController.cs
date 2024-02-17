using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;

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
