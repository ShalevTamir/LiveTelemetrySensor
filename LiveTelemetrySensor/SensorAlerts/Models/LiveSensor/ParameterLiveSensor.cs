using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using PdfExtractor.Models.Enums;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor
{
    public class ParameterLiveSensor: BaseSensor
    {
        public readonly IEnumerable<RequirementModel> Requirements;

        public ParameterLiveSensor(string sensedParamName, IEnumerable<SensorRequirement> additionalRequirements, IEnumerable<RequirementModel> requirements) : base(sensedParamName, additionalRequirements)
        {
            Requirements = requirements;
        }

        public bool Sense(double valueToSense)
        {
            //bug caused because: not updaing duration status when other requirement types are met and then it thinks that the previous is REQUIREMENT_MET
            foreach (RequirementModel requirement in Requirements)
            {
                RequirementParam requirementParam = requirement.RequirementParam;
                if (requirementParam.RequirementMet(valueToSense))
                {
                    if (requirement.Type == RequirementType.INVALID && !AdditionalRequirementMet())
                        return false;

                    return UpdateSensorState(Enum.Parse<SensorState>(requirement.Type.ToString()));
                }
            }

            return false;
        }

        public ParameterSensorDto ToParameterSensorDto()
        {
            return new ParameterSensorDto()
            {
                SensorName = SensedParamName,
                AdditionalRequirements = AdditionalRequirements.Select((additionalRequirement) => additionalRequirement.ToRequirementDto()).ToArray(),
                Requirements = Requirements.Select((requirement) => new RequirementModelDto(requirement)).ToArray()
            };
        }

       
    }
}
