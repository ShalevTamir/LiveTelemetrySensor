using LiveTelemetrySensor.Mongo.Models.Dtos;
using LiveTelemetrySensor.Mongo.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Mongo.Controllers
{
    [Route("mongo-alerts")]
    [ApiController]
    public class MongoAlertsController: Controller
    {
        private MongoAlertsService _mongoAlertsService;

        public MongoAlertsController(MongoAlertsService mongoAlertsService)
        {
            _mongoAlertsService = mongoAlertsService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAlerts([FromQuery]GetAlertsQueryParams queryParams)
        {
            return Ok(JsonConvert.SerializeObject(await _mongoAlertsService.GetAlerts(
                queryParams.MinTimeStamp,
                queryParams.MaxTimeStamp,
                queryParams.MaxSamplesInPage,
                queryParams.PageNumber
                )));
        }
    }
}
