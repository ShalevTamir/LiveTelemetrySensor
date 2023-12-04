using LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement;
using Newtonsoft.Json.Linq;
using PdfExtractor.Models.Requirement;
using LiveTelemetrySensor.SensorAlerts.Models;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class JObjectExtention
    {

        public static RequirementParam ParseAsRequirement(this JObject JObj)
        {
            if (JObj.ContainsKey(Constants.REQUIREMENT_END_VALUE_NAME))
                return new RequirementRange(
                ParseNumericalValue(JObj, Constants.REQUIREMENT_VALUE_NAME),
                ParseNumericalValue(JObj, Constants.REQUIREMENT_END_VALUE_NAME)
                );
            else
                return new RequirementParam(
                    ParseNumericalValue(JObj, Constants.REQUIREMENT_VALUE_NAME)
                    );

        }

        
        private static double ParseNumericalValue(JObject JObj, string valueName)
        {
            string numericalValue = JObj[valueName].ToString();
            switch (numericalValue)
            {
                case Constants.POSITIVE_INIFNITY:
                    return double.PositiveInfinity;
                case Constants.NEGATIVE_INFINITY:
                    return double.NegativeInfinity;
                default:
                    return double.Parse(numericalValue);
            }
        }
    }
}
