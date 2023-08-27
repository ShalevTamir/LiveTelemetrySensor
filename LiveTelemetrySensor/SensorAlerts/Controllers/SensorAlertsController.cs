using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services;
using Microsoft.AspNetCore.Mvc;

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
    }
}
