using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using Newtonsoft.Json.Linq;
using PdfExtractor.Models.Requirement;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System;
using LiveTelemetrySensor.SensorAlerts.Models;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class JObjectExtention
    {

        public static JToken NullSafeIndexing(this JObject JObj, string index)
        {
            JToken? element = JObj[index];
            if (element == null)
            {
                throw new ArgumentException("Tried indexing JObject with index " + index + " but result was null");
            }
            return element;
        }


        public static Duration ParseAsDuration(this JObject JObj)
        {
            DurationType durationType = (DurationType)JObj.Value<int>(SensorAlertsConstants.DURATION_TYPE_NAME);
            RequirementParam requirement = ((JObject) JObj.NullSafeIndexing(SensorAlertsConstants.REQUIREMENT_PARAM_NAME)).ParseAsRequirement();
            return new Duration(durationType, requirement);
        }

        public static RequirementParam ParseAsRequirement(this JObject JObj)
        {
            string sValue = JObj.NullSafeIndexing(SensorAlertsConstants.REQUIREMENT_VALUE_NAME).ToString();
            if (JObj.ContainsKey(SensorAlertsConstants.REQUIREMENT_END_VALUE_NAME))
            {
                string sEndValue = JObj.NullSafeIndexing(SensorAlertsConstants.REQUIREMENT_END_VALUE_NAME).ToString();

                double startValue = sValue.Equals(SensorAlertsConstants.PY_NEG_INF) ? double.NegativeInfinity : double.Parse(sValue);
                double endValue = sEndValue.Equals(SensorAlertsConstants.PY_INF) ? double.PositiveInfinity : double.Parse(sEndValue);
                return new RequirementRange(startValue, endValue);
            }
            else
                return new RequirementParam(double.Parse(sValue));
        }

        
        
    }
}
