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
        private TeleProcessorService _teleProcessor;
        
        public LiveSensorAlertsController(SensorAlertsService sensorAlertsService, TeleProcessorService teleProcessor) 
        {
            _sensorAlerts = sensorAlertsService;
            _teleProcessor = teleProcessor;
        }

        [HttpGet]
        public ActionResult GetSensorsState()
        {
            return Ok(JsonConvert.SerializeObject(_teleProcessor.GetSensorsState().ToArray()));
        }

        [HttpPut("state")]
        public ActionResult ChangeSensorState([FromBody] RunningState stateToChangeTo)
        {
            bool success = _sensorAlerts.ChangeState(stateToChangeTo);
            if (success) return Ok();
            else return BadRequest("Server is already in state " + stateToChangeTo);
        }

        

   
    }
}
