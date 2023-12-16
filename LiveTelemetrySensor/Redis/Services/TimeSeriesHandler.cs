﻿using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using System;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Redis.Services
{
    public class TimeSeriesHandler
    {

        private ITimeSeriesCommands _commands;
        public static string REDIS_EARLIEST_SAMPLE = "-";
        public static string REDIS_LATEST_SAMPLE = "+";
        public TimeSeriesHandler(ITimeSeriesCommands timeSeriesCommands)
        {
            _commands = timeSeriesCommands;
        }
        public void Create(string key,
                                    long? retentionTime = null,
                                    IReadOnlyCollection<TimeSeriesLabel> labels = null,
                                    bool? uncompressed = null,
                                    long? chunkSizeBites = null,
                                    TsDuplicatePolicy? duplicatePolicy = null)
        {
            _commands.Create(key, retentionTime, labels, uncompressed, chunkSizeBites, duplicatePolicy);
        }
        public void Add(string seriesName, long timestamp, double value)
        {
            _commands.Add(seriesName, timestamp, value);
        }

        public TimeSeriesTuple GetLastestSample(string seriesName)
        {
            return _commands.Get(seriesName); 
        }

        public void DeleteRange(string seriesName, long from, long to)
        {
            _commands.Del(seriesName, from, to);
        }

        public TimeSeriesInformation Info(string key)
        {
            return _commands.Info(key);
        }

        public IReadOnlyList<TimeSeriesTuple> GetAllSamples(string key)
        {
            return _commands.Range(key, new TimeStamp(REDIS_EARLIEST_SAMPLE), new TimeStamp(REDIS_LATEST_SAMPLE));
        }

        public IReadOnlyList<TimeSeriesTuple> GetRange(string key, long from, long to)
        {
            return _commands.Range(key, from, to);
        }

        public IReadOnlyList<TimeSeriesTuple> GetReverseRange(string key, long from, long to)
        {
            return _commands.RevRange(key, from, to);
        }

        // Returns the range relative to the latest sample - offsets are in milliseconds
        public IReadOnlyList<TimeSeriesTuple> GetRelativeRange(string key, long fromOffset = 0, long toOffset = 0)
        {
            TimeSeriesTuple latestSample = GetLastestSample(key);
            
            return _commands.Range(key, latestSample.Time - fromOffset, latestSample.Time - toOffset);
        }


        public bool RetentionReached(string key, long? currentTimestamp = null, long? retentionTime = null)
        {
            TimeSeriesInformation info = Info(key);
            retentionTime ??= info.RetentionTime;
            currentTimestamp ??= info.LastTimeStamp;

            return info.FirstTimeStamp != null &&
                   currentTimestamp != null &&
                   currentTimestamp - info.FirstTimeStamp >= retentionTime;
        }


    }
}