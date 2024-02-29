using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class RequirementRangeExtention
    {
        public static bool IsValidRange(this RequirementRange range)
        {
            return range.EndValue >= range.Value;
        }
    }
}
