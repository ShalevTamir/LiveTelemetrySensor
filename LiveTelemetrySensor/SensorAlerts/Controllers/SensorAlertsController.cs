using LiveTelemetrySensor.Redis.Interfaces;
using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SensorAlertsController : Controller
    {
        private SensorAlertsService _sensorAlerts;
        private RedisCacheService _redisCacheService;
        public SensorAlertsController(SensorAlertsService sensorAlertsService, RedisCacheService redis) 
        {
            _sensorAlerts = sensorAlertsService;
            _redisCacheService = redis;
        }
        [HttpPut("state")]
        public ActionResult ChangeSensorState([FromBody] RunningState stateToChangeTo)
        {
            Debug.WriteLine("======================================\nCACHE SERVICE\n==================================");

            //bool success = _sensorAlerts.ChangeState(stateToChangeTo);
            //if (success) return Ok();
            //else return BadRequest("Server is already in state " + stateToChangeTo);
            return Ok();
        }
    }
}
