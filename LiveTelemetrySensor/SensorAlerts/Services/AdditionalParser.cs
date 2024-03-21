using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class AdditionalParser
    {
        private string ADDITIONAL_PARSER_URI = "http://127.0.0.1:5000/sensor/";
        private RequestsService _requestsService;
        public AdditionalParser(RequestsService requestService) 
        {
            _requestsService = requestService;
        }

        public async Task<SensorRequirement[]> Parse(string additionalText)
        {
            try
            { 
                string JSensors = await _requestsService.PostAsync(
                     ADDITIONAL_PARSER_URI,
                     new AdditionalTextDto() { text = additionalText }
                );
                JArray JObjSensors = JArray.Parse(JSensors);

                return JObjSensors.Select(JObjSensor =>
                {
                    string parameterName = JObjSensor.NullSafeIndexing(Constants.SENSOR_PARAM_NAME).ToString();
                    JObject requirementParam = (JObject) JObjSensor.NullSafeIndexing(Constants.REQUIREMENT_PARAM_NAME);
                    JObject? duration = (JObject?)JObjSensor[Constants.DURATION_PARAM_NAME];
                    var sensor = new SensorRequirement(
                        parameterName,
                        requirementParam.ParseAsRequirement(),
                        duration?.ParseAsDuration()
                        );
                    //Debug.WriteLine(JsonConvert.SerializeObject(sensor));
                    return sensor;
                }).ToArray();
            }
            catch(HttpRequestException e)
            {
                Debug.WriteLine(e);
                return new SensorRequirement[] { };
            }
        }
    }
}
