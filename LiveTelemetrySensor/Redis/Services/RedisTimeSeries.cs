using LiveTelemetrySensor.Redis.Interfaces;
using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System;
using NRedisTimeSeries;
using System.Collections.Generic;
using System.Linq;
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
            _seriesKey = key;

            if (database.KeyExists(key))
            {
                long existingRetentionTime = Info().RetentionTime;
                long newRetentionTime = retentionTime.HasValue ? Math.Max(existingRetentionTime, (long)retentionTime) : existingRetentionTime;
                _commands.Alter(key, newRetentionTime, chunkSizeBites, duplicatePolicy, labels);
            }
            else
                _commands.Create(key, retentionTime, labels, uncompressed, chunkSizeBites, duplicatePolicy);

            LatestDeletedSample = null;
        }

        public TimeSeriesTuple? NextSampleAfterRetention(long? retention = null, TimeStamp? relativeFromTimestamp = null)
        {
            TimeSeriesInformation info = Info();
            relativeFromTimestamp ??= info.LastTimeStamp;
            retention ??= info.RetentionTime;
            var samples = IsValidTimestamp(relativeFromTimestamp) ?
                GetReverseRange(REDIS_EARLIEST_SAMPLE, relativeFromTimestamp - retention - 1, count : 1) :
                Enumerable.Empty<TimeSeriesTuple>();
            return samples.Count() > 0 ? samples.First() : LatestDeletedSample;
        }

        public void Add(TimeStamp timestamp, double value)
        {
            if (RetentionReached(timestamp))
                LatestDeletedSample = NextSampleAfterRetention(relativeFromTimestamp: timestamp);
     
            _commands.Add(_seriesKey, timestamp, value);
        }



        // O(1)
        public TimeSeriesTuple? GetFirstSample()
        {
            TimeStamp? firstTimeStamp = Info().FirstTimeStamp;
            return IsValidTimestamp(firstTimeStamp) ? GetRange(firstTimeStamp, firstTimeStamp)[0] : null;
        }

        // O(1)
        public TimeSeriesTuple? GetLastestSample()
        {
            return _commands.Get(_seriesKey); 
        }

        // O(M) - M amount of samples to delete
        public void DeleteRange(TimeStamp from, TimeStamp to)
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
        public IReadOnlyList<TimeSeriesTuple> GetRange(TimeStamp from, TimeStamp to, 
            bool latest = false, 
            IReadOnlyCollection<TimeStamp>? filterByTs = null, 
            (long, long)? filterByValue = null, 
            long? count = null, 
            TimeStamp? align = null, 
            TsAggregation? aggregation = null, 
            long? timeBucket = null, 
            TsBucketTimestamps? bt = null, 
            bool empty = false
            )
        {
            return _commands.Range(_seriesKey, from, to, latest, filterByTs, filterByValue, count, align, aggregation, timeBucket, bt, empty);
        }

        // O(N)
        public IReadOnlyList<TimeSeriesTuple> GetReverseRange(TimeStamp from, TimeStamp to, 
            bool latest = false, 
            IReadOnlyCollection<TimeStamp>? filterByTs = null, 
            (long, long)? filterByValue = null, 
            long? count = null, 
            TimeStamp? align = null, 
            TsAggregation? aggregation = null, 
            long? timeBucket = null, 
            TsBucketTimestamps? bt = null, 
            bool empty = false
            )
        {
            return _commands.RevRange(_seriesKey, from, to, latest, filterByTs, filterByValue, count, align, aggregation, timeBucket, bt, empty);
        }

        // Returns the range relative to the latest sample - offsets are in milliseconds
        // O(N)
        public IReadOnlyList<TimeSeriesTuple>? GetRelativeRange(TimeStamp? fromOffset = null, TimeStamp? toOffset = null)
        {
            TimeSeriesTuple? latestSample = GetLastestSample();

            fromOffset ??= new TimeStamp(0);
            toOffset ??= new TimeStamp(0);
            
            return latestSample != null ? _commands.Range(_seriesKey, latestSample.Time - fromOffset, latestSample.Time - toOffset) : null;
        }

        // O(1)
        public bool RetentionReached(TimeStamp? currentTimestamp = null, TimeStamp? retentionTime = null)
        {
            TimeSeriesInformation info = Info();
            retentionTime ??= info.RetentionTime;
            
            currentTimestamp ??= IsValidTimestamp(info.LastTimeStamp) ? new TimeStamp((long)info.LastTimeStamp.Value) : currentTimestamp;
            TimeStamp? firstTimeStamp = LatestDeletedSample != null ? LatestDeletedSample.Time : info.FirstTimeStamp;

            return IsValidTimestamp(firstTimeStamp) &&
                   IsValidTimestamp(currentTimestamp) &&
                   currentTimestamp - firstTimeStamp >= retentionTime;
        }

        private bool IsValidTimestamp(TimeStamp? timestampToCheck)
        {
            return timestampToCheck != null && (long)timestampToCheck.Value > 0;
        }

    }
}
