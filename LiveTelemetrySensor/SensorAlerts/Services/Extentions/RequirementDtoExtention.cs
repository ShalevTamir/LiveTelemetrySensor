using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class RequirementDtoExtention
    {
        public static RequirementParam ToRequirementParam(this RequirementDto requirementDto)
        {
            return requirementDto.EndValue == null ?
                new RequirementParam(ParseValue(requirementDto.Value)) :
                new RequirementRange(ParseValue(requirementDto.Value), ParseValue(requirementDto.EndValue));
        }
        private static double ParseValue(string value)
        {
            if (value == "Infinity") return double.PositiveInfinity;
            if (value == "-Infinity") return double.NegativeInfinity;
            return double.Parse(value);
        }
    }
}
