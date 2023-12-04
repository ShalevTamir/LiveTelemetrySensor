using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json.Linq;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class AdditionalParser
    {
        private string ADDITIONAL_PARSER_URI = "http://127.0.0.1:5000/sensor";
        private RequestsService _requestsService;
        public AdditionalParser(RequestsService requestService) 
        {
            _requestsService = requestService;
        }

        public SensorRequirement[] Parse(string additionalText)
        {
            SensorRequirement[] requirementParams;
            try
            { 
                string JSensors = _requestsService.PostAsync(
                     ADDITIONAL_PARSER_URI,
                     new SentenceDto() { text = additionalText }
                ).Result;
                JArray keyValuePairs = JArray.Parse(JSensors);
                requirementParams = new SensorRequirement[keyValuePairs.Count];
                for ( int i = 0; i < keyValuePairs.Count; i++)
                {
                    var currentPair = keyValuePairs[i];
                    JObject requirementParam = currentPair[Constants.REQUIREMENT_PARAM_NAME] as JObject;
                    requirementParams[i] = new SensorRequirement(currentPair[Constants.SENSOR_PARAM_NAME].ToString(), requirementParam.ParseAsRequirement());
                }
            }
            catch(AggregateException ae)
            {
                ae.HandleExceptions(typeof(HttpRequestException));
                return null;
            }
            return requirementParams;
        }
    }
}
