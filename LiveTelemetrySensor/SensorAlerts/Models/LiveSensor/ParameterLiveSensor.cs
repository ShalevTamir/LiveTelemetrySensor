using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using PdfExtractor.Models.Enums;
using PdfExtractor.Models.Requirement;
using System;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor
{
    public class ParameterLiveSensor: BaseSensor
    {
        public readonly IEnumerable<RequirementModel> Requirements;

        public ParameterLiveSensor(string sensedParamName, IEnumerable<SensorRequirement> additionalRequirements, IEnumerable<RequirementModel> requirements) : base(sensedParamName, additionalRequirements)
        {
            Requirements = requirements;
        }

        public bool Sense(double valueToSense, Func<SensorRequirement, DurationStatus> UpdateDurationStatus)
        {
            foreach (RequirementModel requirement in Requirements)
            {
                RequirementParam requirementParam = requirement.RequirementParam;
                if (requirementParam.RequirementMet(valueToSense))
                {
                    if (requirement.Type == RequirementType.INVALID && !AdditionalRequirementMet(UpdateDurationStatus))
                        return false;

                    return UpdateSensorState(Enum.Parse<SensorState>(requirement.Type.ToString()));
                }
            }

            return false;
        }

       
    }
}
