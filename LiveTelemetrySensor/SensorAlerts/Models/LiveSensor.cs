using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.Redis.Services;

namespace LiveTelemetrySensor.SensorAlerts.Models
{
    public class LiveSensor //: ISensor<double, TelemetryParameterDto>
    {
        public readonly string SensedParamName;
        public readonly IEnumerable<RequirementModel> Requirements;
        public readonly IEnumerable<SensorRequirement> AdditionalRequirements;
        public SensorState CurrentSensorState { get; private set; }


        public LiveSensor(string sensedParamName, IEnumerable<RequirementModel> requirements, IEnumerable<SensorRequirement> additionalRequirements)
        {
            SensedParamName = sensedParamName;
            Requirements = requirements;
            AdditionalRequirements = additionalRequirements;
            CurrentSensorState = SensorState.NEUTRAL;

        }
        public bool Sense(double valueToSense, Func<SensorRequirement, DurationStatus> UpdateDurationStatus)
        {
            if (Requirements.Count() == 0)
            {
                return UpdateSensorState(AdditionalRequirementMet(UpdateDurationStatus) ? SensorState.INVALID : SensorState.NEUTRAL);
            }
            else
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
            }
            return false;
        }

        private bool UpdateSensorState(SensorState newState)
        {
            SensorState previousState = CurrentSensorState;
            CurrentSensorState = newState;
            return previousState != CurrentSensorState;
        }

        private bool AdditionalRequirementMet(Func<SensorRequirement, DurationStatus> UpdateDurationStatus)
        {
            foreach(var sensorRequirement in AdditionalRequirements) 
            { 
                if (UpdateDurationStatus(sensorRequirement) == DurationStatus.REQUIREMENT_NOT_MET)
                    return false;
            }
            
            return true;
        }

        //private TelemetryParameterDto FindParameter(IEnumerable<TelemetryParameterDto> parameterValues, string parameterName)
        //{
        //    foreach (var parameter in parameterValues)
        //    {
        //        if (parameter.Name.ToLower() == parameterName)
        //            return parameter;
        //    }
        //    return null;
        //}
    }
}
