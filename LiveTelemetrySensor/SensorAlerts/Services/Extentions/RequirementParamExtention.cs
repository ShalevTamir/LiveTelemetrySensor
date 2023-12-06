using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class RequirementParamExtention
    {
        public static bool RequirementMet(this RequirementParam requirement, double value)
        {
            if (requirement is RequirementRange)
            {
                RequirementRange requirementRange = (RequirementRange)requirement;
                if (value < requirementRange.EndValue && value >= requirementRange.Value)
                    return true;
            }
            else if (value == requirement.Value)
                return true;
            return false;
        }
    }
}
