using LiveTelemetrySensor.Redis.Interfaces;
using Microsoft.Extensions.Configuration;
using NRedisStack;
using NRedisStack.DataTypes;
using NRedisStack.Literals.Enums;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Redis.Services
{
    public class RedisCacheService
    {
        public IDatabase DataBase { get; }
        private IConnectionMultiplexer _connectionMultiplexer;
        private IConfiguration _configuration;

        // key - time series key, value - the corresponding RedisTimeSeries related
        public Dictionary<string, IRedisDataType> _storedObjects = new Dictionary<string, IRedisDataType>();

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
        {
            DataBase = connectionMultiplexer.GetDatabase();
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public RedisTimeSeries CreateTimeSeries(string key,
                                    long? retentionTime = null,
                                    IReadOnlyCollection<TimeSeriesLabel>? labels = null,
                                    bool? uncompressed = null,
                                    long? chunkSizeBites = null,
                                    TsDuplicatePolicy? duplicatePolicy = null)
        {
            var instance = new RedisTimeSeries(DataBase, key, retentionTime, labels, uncompressed, chunkSizeBites, duplicatePolicy);
            _storedObjects[key] = instance;
            return instance;
        }

        public DataType GetStoredObject<DataType>(string key)
        {
            return (DataType) _storedObjects[key];
        }

        public IRedisDataType GetStoredObject(string key)
        {
            return _storedObjects[key];
        }

        public bool HasTimeSeries(string key)
        {
            return _storedObjects.ContainsKey(key);
        }


        public bool ContainsKey(string key)
        {
            return DataBase.KeyExists(key);
        }

        public void FlushAll()
        {
            _connectionMultiplexer.GetServer(_configuration["Redis:Configuration:ServerAdress"]).FlushDatabase();
        }

    }
}
