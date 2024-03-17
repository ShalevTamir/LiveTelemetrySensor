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
using Newtonsoft.Json;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor
{
    public abstract class BaseSensor //: ISensor<double, TelemetryParameterDto>
    {
        public readonly string SensedParamName;
        public readonly IEnumerable<SensorRequirement> AdditionalRequirements;
        [JsonIgnore]
        public SensorState CurrentSensorState { get; private set; }


        public BaseSensor(string sensedParamName, IEnumerable<SensorRequirement> additionalRequirements)
        {
            SensedParamName = sensedParamName.ToLower();
            AdditionalRequirements = additionalRequirements;
            CurrentSensorState = SensorState.NORMAL;
        }

        protected bool UpdateSensorState(SensorState newState)
        {
            SensorState previousState = CurrentSensorState;
            CurrentSensorState = newState;
            return previousState != CurrentSensorState;
        }

        protected bool AdditionalRequirementMet()
        {
            return AdditionalRequirements.All((sensorRequirement) => sensorRequirement.RequirementStatus == RequirementStatus.REQUIREMENT_MET);
        }

        public void UpdateAdditionalRequirementStatus(Func<SensorRequirement, RequirementStatus> UpdateRequirementStatusCallback)
        {
            foreach(var sensorRequirement in AdditionalRequirements)
            {
                UpdateRequirementStatusCallback(sensorRequirement);
            }
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
