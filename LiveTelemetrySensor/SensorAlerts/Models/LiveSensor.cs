using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.Interfaces;
using PdfExtractor.Models.Requirement;
using PdfExtractor.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models
{
    public class LiveSensor : ISensor<double, TelemetryParameterDto>
    {
        public readonly string SensedParamName;
        public readonly RequirementModel[] Requirements;
        public readonly SensorRequirement[] AdditionalRequirements;
        public SensorState CurrentSensorState { get; private set; }

        public LiveSensor(string sensedParamName, RequirementModel[] requirements, SensorRequirement[] additionalRequirements)
        {
            SensedParamName = sensedParamName;
            Requirements = requirements;
            AdditionalRequirements = additionalRequirements;
            CurrentSensorState = SensorState.NEUTRAL;
        }

        
        public bool Sense(double valueToSense, IEnumerable<TelemetryParameterDto> parameterValues)
        {
            foreach (RequirementModel requirement in Requirements)
            {
                RequirementParam requirementParam = requirement.RequirementParam;
                if (RequirementMet(requirementParam, valueToSense))
                {
                    if (requirement.Type == RequirementType.INVALID && !AdditionalRequirementMet(parameterValues))
                        return false;

                    SensorState previousState = CurrentSensorState;
                    CurrentSensorState = Enum.Parse<SensorState>(requirement.Type.ToString());
                    return previousState != CurrentSensorState;
                }
            }
            return false;
        }

        private bool AdditionalRequirementMet(IEnumerable<TelemetryParameterDto> parameterValues)
        {
            foreach(var sensorRequirement in  AdditionalRequirements)
            {
                TelemetryParameterDto parameter = FindParameter(parameterValues, sensorRequirement.ParameterName);
                if (parameter != null)
                {
                    if (!RequirementMet(sensorRequirement.Requirement, double.Parse(parameter.Value)))
                        return false;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Sensor " +
                        sensorRequirement.ParameterName + 
                        " doesn't exist in the frame, and is required by sensor " 
                        + SensedParamName);
                }
            }
            return true;
        }

        private bool RequirementMet(RequirementParam requirement, double value)
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

        private TelemetryParameterDto FindParameter(IEnumerable<TelemetryParameterDto> parameterValues, string parameterName)
        {
            foreach (var parameter in parameterValues)
            {
                if (parameter.Name.ToLower() == parameterName)
                    return parameter;
            }
            return null;
        }
    }
}
