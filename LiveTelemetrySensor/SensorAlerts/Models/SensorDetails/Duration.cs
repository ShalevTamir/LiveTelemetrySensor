﻿using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.SensorDetails
{
    public class Duration
    {
        public DurationType DurationType { get; set; }
        public RequirementParam RequirementParam { get; set; }

        public RequirementStatus RequirementStatus { get; set; }
            
        public Duration(DurationType durationType, RequirementParam requirementParam)
        {
            DurationType = durationType;
            RequirementParam = requirementParam;
            RequirementStatus = RequirementStatus.REQUIREMENT_NOT_MET;
        }
    }
}
