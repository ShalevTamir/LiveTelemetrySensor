﻿using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class TeleProcessorService
    {
        private Dictionary<string,LiveSensor> _liveSensors;
        private CommunicationService _communicationService;
        private RedisCacheHandler _redisCacheHandler;

        public TeleProcessorService(CommunicationService communicationService, RedisCacheHandler redisCacheHandler)
        {
            _communicationService = communicationService;
            _liveSensors = new Dictionary<string, LiveSensor>();
            _redisCacheHandler = redisCacheHandler;
        }
        public void AddSensorsToUpdate(IEnumerable<LiveSensor> liveSensors)
        {
            foreach (var liveSensor in liveSensors)
                _liveSensors.Add(liveSensor.SensedParamName,liveSensor);
            _redisCacheHandler.AddRelevantSensors(liveSensors);
        }
       
        public void ProcessTeleData(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);
            foreach(var teleParam in telemetryFrame.Parameters)
            {
                string teleParamName = teleParam.Name.ToLower();
                if (!_liveSensors.ContainsKey(teleParamName)) continue;
                LiveSensor liveSensor = _liveSensors[teleParamName];
                bool stateUpdated = liveSensor.Sense(double.Parse(teleParam.Value), telemetryFrame.TimeStamp, _redisCacheHandler.CacheParameter);
                if (stateUpdated)
                {
                    _communicationService.SendSensorAlert(new SensorAlertDto()
                    {
                        SensorName = liveSensor.SensedParamName,
                        CurrentStatus = liveSensor.CurrentSensorState
                    });
                    telemetryFrame.Parameters.ToList().ForEach(p => Debug.WriteLine(p.Name + " " + p.Value));
                }
            }
        }

       
    }
}
