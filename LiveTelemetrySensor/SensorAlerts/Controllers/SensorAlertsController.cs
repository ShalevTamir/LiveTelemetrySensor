using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveTelemetrySensor.SensorAlerts.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SensorAlertsController : Controller
    {
        public SensorAlertsController(SensorPropertiesService sensorsPropertiesService) 
        {
        }
        [HttpPut("state")]
        public ActionResult ChangeSensorState([FromBody] RunningState stateToChangeTo)
        {
            
            //bool success = _cons.ChangeState(stateToChangeTo);
            //if (success) return Ok();
            //return BadRequest($"Sensor alerts are already in state {stateToChangeTo}");
            return Ok();
        }
    }
}
