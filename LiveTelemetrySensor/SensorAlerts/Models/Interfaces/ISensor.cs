using System.Collections;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Models.Interfaces
{
    public interface ISensor<SensedValueType, ParameterType>
    {
        /// <summary>
        /// Senses a change in the live sensor
        /// </summary>
        /// <returns>
        /// <para>true - if changed state</para>
        /// <para>false - if the current state equals to the given state</para>
        /// </returns>
        bool Sense(SensedValueType valueToSense, IEnumerable<ParameterType> values);
    }
}
