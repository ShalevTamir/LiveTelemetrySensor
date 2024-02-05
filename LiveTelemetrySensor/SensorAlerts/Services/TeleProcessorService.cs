﻿using LiveTelemetrySensor.Common.Extentions;
using LiveTelemetrySensor.Mongo.Models;
using LiveTelemetrySensor.Mongo.Services;
using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class TeleProcessorService
    {
        private Dictionary<string,LiveSensor> _liveSensors;
        private CommunicationService _communicationService;
        private RedisCacheHandler _redisCacheHandler;
        private MongoAlertsService _mongoAlertsService;

        public TeleProcessorService(CommunicationService communicationService, RedisCacheHandler redisCacheHandler, MongoAlertsService mongoAlertsService)
        {
            _communicationService = communicationService;
            _liveSensors = new Dictionary<string, LiveSensor>();
            _redisCacheHandler = redisCacheHandler;
            _mongoAlertsService = mongoAlertsService;
        }
        public void AddSensorsToUpdate(IEnumerable<LiveSensor> liveSensors)
        {
            foreach (var liveSensor in liveSensors)
            {
                AddSensorToUpdate(liveSensor);
            }
        }

        public void AddSensorToUpdate(LiveSensor liveSensor)
        {
            if (_liveSensors.ContainsKey(liveSensor.SensedParamName))
                throw new ArgumentException("Sensor " + liveSensor.SensedParamName + " already exists, can't have 2 of the same sensor");

            _liveSensors.Add(liveSensor.SensedParamName, liveSensor);
            _redisCacheHandler.AddRelevantRequirements(liveSensor);
        }
       
        public async Task ProcessTeleDataAsync(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);
            if (telemetryFrame == null)
                throw new ArgumentException("Unable to deserialize telemetry frame \n" + JTeleData+"");
            _redisCacheHandler.CacheTeleData(telemetryFrame);
            foreach (var teleParam in telemetryFrame.Parameters)
            {
                string teleParamName = teleParam.Name.ToLower();
                if (!_liveSensors.ContainsKey(teleParamName)) continue;
                LiveSensor liveSensor = _liveSensors[teleParamName];
                bool stateUpdated = liveSensor.Sense(double.Parse(teleParam.Value), _redisCacheHandler.UpdateDurationStatus);
                if (stateUpdated)
                {
                    await _communicationService.SendSensorAlertAsync(new SensorAlertDto()
                    {
                        SensorName = liveSensor.SensedParamName,
                        CurrentStatus = liveSensor.CurrentSensorState
                    });

                    await _mongoAlertsService.InsertAlert(new Alert()
                    {
                        SensorName = liveSensor.SensedParamName,
                        SensorStatus = liveSensor.CurrentSensorState,
                        TimeStamp = telemetryFrame.TimeStamp.ToUnix()
                    });
                    //telemetryFrame.Parameters.ToList().ForEach(p => Debug.WriteLine(p.Name + " " + p.Value));
                }
            }
        }

       
    }
}
