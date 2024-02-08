﻿using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
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

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor
{
    public abstract class BaseSensor //: ISensor<double, TelemetryParameterDto>
    {
        public readonly string SensedParamName;
        public readonly IEnumerable<SensorRequirement> AdditionalRequirements;
        public SensorState CurrentSensorState { get; private set; }


        public BaseSensor(string sensedParamName, IEnumerable<SensorRequirement> additionalRequirements)
        {
            SensedParamName = sensedParamName;
            AdditionalRequirements = additionalRequirements;
            CurrentSensorState = SensorState.NEUTRAL;
        }

        protected bool UpdateSensorState(SensorState newState)
        {
            SensorState previousState = CurrentSensorState;
            CurrentSensorState = newState;
            return previousState != CurrentSensorState;
        }

        protected bool AdditionalRequirementMet(Func<SensorRequirement, DurationStatus> UpdateDurationStatus)
        {
            foreach (var sensorRequirement in AdditionalRequirements)
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