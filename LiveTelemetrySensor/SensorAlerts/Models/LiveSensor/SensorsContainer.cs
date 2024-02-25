﻿using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor
{
    public class SensorsContainer
    {
        private Dictionary<string, ParameterLiveSensor> _parameterLiveSensors;
        private List<DynamicLiveSensor> _dynamicLiveSensors;

        public SensorsContainer() 
        { 
            _parameterLiveSensors = new Dictionary<string, ParameterLiveSensor>();
            _dynamicLiveSensors = new List<DynamicLiveSensor>();
        }

        public void InsertSensor(BaseSensor sensor)
        {
            if (sensor is ParameterLiveSensor parameterSensor)
                _parameterLiveSensors.Add(sensor.SensedParamName.ToLower(), parameterSensor);
            else if (sensor is DynamicLiveSensor dynamicLiveSensor)
                _dynamicLiveSensors.Add(dynamicLiveSensor);
        }

        public bool RemoveSensor(string sensorName)
        {
            int beforeRemovalCount = _parameterLiveSensors.Count + _dynamicLiveSensors.Count;
            _parameterLiveSensors.Remove(sensorName);
            _dynamicLiveSensors.RemoveAll((sensor) => sensor.SensedParamName == sensorName);
            int currentCount = _parameterLiveSensors.Count + _dynamicLiveSensors.Count;
            return beforeRemovalCount != currentCount;
        }

        public bool hasSensor(BaseSensor sensor)
        {
            return hasSensor(sensor.SensedParamName);  
        }

        public bool hasSensor(string sensorName)
        {
            return _parameterLiveSensors.ContainsKey(sensorName)
                || _dynamicLiveSensors.Any((sensor) => sensor.SensedParamName == sensorName);

        }

        public ParameterLiveSensor GetParameterLiveSensor(string sensorName)
        {
            return _parameterLiveSensors[sensorName];
        }

        public IEnumerable<DynamicLiveSensor> GetDynamicLiveSensors()
        {
            return _dynamicLiveSensors;
        }

        public IEnumerable<BaseSensor> GetAllSensors()
        {
            IEnumerable<BaseSensor> allSensors = Enumerable.Empty<BaseSensor>();
            allSensors = allSensors.Concat(_dynamicLiveSensors).Concat(_parameterLiveSensors.Values);
            return allSensors;
        }
    }
}
