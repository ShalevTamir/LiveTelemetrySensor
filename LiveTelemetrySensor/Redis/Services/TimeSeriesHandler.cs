using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using System;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Redis.Services
{
    public class TimeSeriesHandler
    {

        private ITimeSeriesCommands _commands;
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
        public void Add(string seriesName, DateTime timestamp, double value)
        {
            _commands.Add(seriesName, timestamp, value);
        }

        public TimeSeriesTuple GetLastSample(string seriesName)
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
            return _commands.Range(key, new TimeStamp("-"), new TimeStamp("+"));
        }

        public IReadOnlyList<TimeSeriesTuple> GetRange(string key, long from, long to)
        {
            return _commands.Range(key, from, to);
        }

        public bool DurationReached(string key, long duration)
        {
            TimeSeriesInformation info = Info(key);
            return info.LastTimeStamp - info.FirstTimeStamp >= duration;
        }


    }
}
