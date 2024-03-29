﻿using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using System;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor
{
    public class DynamicLiveSensor: BaseSensor
    {
        public DynamicLiveSensor(string sensedParamName, IEnumerable<SensorRequirement> additionalRequirements) : base(sensedParamName, additionalRequirements)
        {
        }

        public bool Sense()
        {
            return UpdateSensorState(AdditionalRequirementMet() ? SensorState.INVALID : SensorState.VALID);
        }
    }
}
