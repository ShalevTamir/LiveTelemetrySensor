using LiveTelemetrySensor.Mongo.Models.Dtos;
using LiveTelemetrySensor.Mongo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Mongo.Controllers
{
    [Authorize]
    [Route("mongo-alerts")]
    [ApiController]
    public class MongoAlertsController: Controller
    {
        private MongoAlertsService _mongoAlertsService;

        public MongoAlertsController(MongoAlertsService mongoAlertsService)
        {
            _mongoAlertsService = mongoAlertsService;
        }

        [HttpGet("count")]
        public async Task<ActionResult> CountAlerts([Required] long MinTimeStamp, [Required] long MaxTimeStamp)
        {
            return Ok(JsonConvert.SerializeObject(new {
                Count = await _mongoAlertsService.CountAlerts(MinTimeStamp,MaxTimeStamp)
                }));
        }

        [HttpGet]
        public async Task<ActionResult> GetAlerts([Required]
                                                    long MinTimeStamp,
                                                    [Required]
                                                    long MaxTimeStamp,
                                                    [Required]
                                                    int MaxSamplesInPage,
                                                    [Required]
                                                    int PageNumber)
        {
            return Ok(JsonConvert.SerializeObject(await _mongoAlertsService.GetAlerts(
                MinTimeStamp,
                MaxTimeStamp,
                MaxSamplesInPage,
                PageNumber
                )));
        }
    }
}
