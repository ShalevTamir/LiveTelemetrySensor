using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using PdfExtractor.Models.Requirement;
using System;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class RequirementParamExtention
    {
        public static bool RequirementMet(this RequirementParam requirement, double value)
        {
            if (requirement is RequirementRange requirementRange)
            {
                if (value < requirementRange.EndValue && value >= requirementRange.Value)
                    return true;
            }
            else if (value == requirement.Value)
                return true;
            return false;
        }

        // Returns - 0 if equal, 1 if greater than comparison, -1 if smaller than comparison
        public static int Compare(this RequirementParam requirement, RequirementParam requirementToCompare)
        {
            return CompareValues(ValueToCompare(requirement), ValueToCompare(requirementToCompare));
        }

        public static bool IsValid(this RequirementParam requirement)
        {
            if (requirement is RequirementRange requirementRange)
                return requirementRange.IsValidRange();
            else
                return true;
        }

        private static double ValueToCompare(RequirementParam requirement)
        {
            if (requirement is RequirementRange)
                return ((RequirementRange)requirement).EndValue;
            
            else 
                return requirement.Value;
        }

        private static int CompareValues(double value1, double value2)
        {
            if(value1 < value2) return -1;
            if(value1 > value2) return 1;
            return 0;
        }

    }
}
