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
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("live-sensor-alerts")]
    [ApiController]
    public class LiveSensorAlertsController : Controller
    {
        private SensorAlertsService _sensorAlerts;
        private SensorsContainer _sensorsContainer;
        
        public LiveSensorAlertsController(SensorAlertsService sensorAlertsService, SensorsContainer sensorsContainer) 
        {
            _sensorAlerts = sensorAlertsService;
            _sensorsContainer = sensorsContainer;
        }

        [HttpGet]
        public ActionResult GetSensorsState()
        {
            return Ok(JsonConvert.SerializeObject(
                _sensorsContainer.GetAllSensors().Select((sensor) => new SensorAlertDto()
            {
                SensorName = sensor.SensedParamName,
                CurrentStatus = sensor.CurrentSensorState
            }).ToArray()));
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
