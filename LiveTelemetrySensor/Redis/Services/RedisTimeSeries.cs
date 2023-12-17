using LiveTelemetrySensor.Redis.Interfaces;
using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System;
using NRedisTimeSeries;
using System.Collections.Generic;
namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisTimeSeries : IRedisDataType
    {

        private ITimeSeriesCommands _commands;
        private string _seriesKey;
        public TimeSeriesTuple? LatestDeletedSample { get; private set; }


        public static TimeStamp REDIS_EARLIEST_SAMPLE = new TimeStamp("-");
        public static TimeStamp REDIS_LATEST_SAMPLE = new TimeStamp("+");

        
        public RedisTimeSeries(IDatabase database, string key, 
                                    long? retentionTime = null,
                                    IReadOnlyCollection<TimeSeriesLabel>? labels = null,
                                    bool? uncompressed = null,
                                    long? chunkSizeBites = null,
                                    TsDuplicatePolicy? duplicatePolicy = null)
        {

            _commands = database.TS();

            if (database.KeyExists(key))
            {
                long existingRetentionTime = Info().RetentionTime;
                long newRetentionTime = retentionTime.HasValue ? Math.Max(existingRetentionTime, (long)retentionTime) : existingRetentionTime;
                _commands.Alter(key, newRetentionTime, chunkSizeBites, duplicatePolicy, labels);
            }
            else
                _commands.Create(key, retentionTime, labels, uncompressed, chunkSizeBites, duplicatePolicy);

            _seriesKey = key;
            LatestDeletedSample = null;
        }

        
        public void Add(long timestamp, double value)
        {
            if (RetentionReached(timestamp))
            {
                var samples = GetRange(REDIS_EARLIEST_SAMPLE, timestamp - Info().RetentionTime);
                LatestDeletedSample = samples.Count > 0 ? samples[samples.Count - 1] : LatestDeletedSample;
            }
                
            _commands.Add(_seriesKey, timestamp, value);
        }



        // O(1)
        public TimeSeriesTuple? GetFirstSample()
        {
            TimeStamp? firstTimeStamp = Info().FirstTimeStamp;
            return firstTimeStamp != null ? GetRange(firstTimeStamp, firstTimeStamp)[0] : null;
        }

        // O(1)
        public TimeSeriesTuple? GetLastestSample()
        {
            return _commands.Get(_seriesKey); 
        }

        // O(M) - M amount of samples to delete
        public void DeleteRange(long from, long to)
        {
            _commands.Del(_seriesKey, from, to);
        }
        
        // O(1)
        public TimeSeriesInformation Info()
        {
            return _commands.Info(_seriesKey);
        }

        // O(N)
        public IReadOnlyList<TimeSeriesTuple> GetAllSamples()
        {
            return _commands.Range(_seriesKey, REDIS_EARLIEST_SAMPLE, REDIS_LATEST_SAMPLE);
        }

        // O(N)
        public IReadOnlyList<TimeSeriesTuple> GetRange(long from, long to)
        {
            return _commands.Range(_seriesKey, from, to);
        }

        // O(N)
        public IReadOnlyList<TimeSeriesTuple> GetReverseRange(long from, long to)
        {
            return _commands.RevRange(_seriesKey, from, to);
        }

        // Returns the range relative to the latest sample - offsets are in milliseconds
        // O(N)
        public IReadOnlyList<TimeSeriesTuple>? GetRelativeRange(long fromOffset = 0, long toOffset = 0)
        {
            TimeSeriesTuple? latestSample = GetLastestSample();
            
            return latestSample != null ? _commands.Range(_seriesKey, latestSample.Time - fromOffset, latestSample.Time - toOffset) : null;
        }

        // O(1)
        public bool RetentionReached(long? currentTimestamp = null, long? retentionTime = null)
        {
            TimeSeriesInformation info = Info();
            retentionTime ??= info.RetentionTime;
            currentTimestamp ??= info.LastTimeStamp != null ? (long)info.LastTimeStamp.Value : currentTimestamp;
            TimeStamp? firstTimeStamp = LatestDeletedSample != null ? LatestDeletedSample.Time : info.FirstTimeStamp;

            return firstTimeStamp != null &&
                   currentTimestamp != null &&
                   currentTimestamp - firstTimeStamp >= retentionTime;
        }


    }
}
