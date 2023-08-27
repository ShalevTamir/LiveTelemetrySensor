using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using PdfExtractor.Models.Requirement;
using System;

namespace LiveTelemetrySensor.SensorAlerts.Models
{
    public class LiveSensor : ISensor<string>
    {
        public readonly string SensedParamName;
        public readonly RequirementModel[] Requirements;
        public readonly string AdditionalRequirement;
        public SensorState CurrentSensorState { get; private set; }

        public LiveSensor(string sensedParamName, RequirementModel[] requirements, string additionalRequirement)
        {
            SensedParamName = sensedParamName;
            Requirements = requirements;
            AdditionalRequirement = additionalRequirement;
            CurrentSensorState = SensorState.NEUTRAL;
        }

        
        public bool Sense(string valueToSense)
        {
            double teleValue = double.Parse(valueToSense);
            foreach (RequirementModel requirement in Requirements)
            {
                RequirementParam requirementParam = requirement.RequirementParam;
                if (requirementParam is RequirementRange)
                {
                    RequirementRange requirementRange = (RequirementRange)requirementParam;
                    if (teleValue < requirementRange.Value || teleValue > requirementRange.EndValue)
                        continue;
                }
                else if (teleValue != requirementParam.Value)
                    continue;

                SensorState previousState = CurrentSensorState;
                CurrentSensorState = Enum.Parse<SensorState>(requirement.Type.ToString());
                return previousState != CurrentSensorState;
                    
            }
            return false;
        }
    }
}
