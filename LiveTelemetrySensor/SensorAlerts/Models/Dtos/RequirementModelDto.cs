using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using PdfExtractor.Models.Enums;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class RequirementModelDto
    {
        public RequirementDto RequirementParam { get; set; }
        public RequirementType Type { get; set; }

        public RequirementModelDto()
        {

        }

        public RequirementModelDto(RequirementModel requirement)
        {
            RequirementParam = new RequirementDto(requirement.RequirementParam);
            Type = requirement.Type;  
        }

        public RequirementModel ToRequirementModel()
        {
            return new RequirementModel(Type, RequirementParam.ToRequirementParam());
        }
    }
}
