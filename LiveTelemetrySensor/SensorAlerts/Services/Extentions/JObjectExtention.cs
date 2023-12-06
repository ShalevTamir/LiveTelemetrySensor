using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using Newtonsoft.Json.Linq;
using PdfExtractor.Models.Requirement;
using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class JObjectExtention
    {

        public static Duration ParseAsDuration(this JObject JObj)
        {
            DurationType durationType = (DurationType)JObj.Value<int>(Constants.DURATION_TYPE_NAME);
            RequirementParam requirement = (JObj[Constants.REQUIREMENT_PARAM_NAME] as JObject).ParseAsRequirement();
            return new Duration(durationType, requirement);
        }

        public static RequirementParam ParseAsRequirement(this JObject JObj)
        {
            string sValue = JObj[Constants.REQUIREMENT_VALUE_NAME].ToString();
            if (JObj.ContainsKey(Constants.REQUIREMENT_END_VALUE_NAME))
            {
                string sEndValue = JObj[Constants.REQUIREMENT_END_VALUE_NAME].ToString();

                double startValue = sValue.Equals(Constants.PY_NEG_INF) ? double.NegativeInfinity : double.Parse(sValue);
                double endValue = sEndValue.Equals(Constants.PY_INF) ? double.PositiveInfinity : double.Parse(sEndValue);
                return new RequirementRange(startValue, endValue);
            }
            else
                return new RequirementParam(double.Parse(sValue));
        }

        
        
    }
}
