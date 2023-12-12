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
        private IDatabase _dataBase;
        private IConnectionMultiplexer _connectionMultiplexer;
        private IConfiguration _configuration;
        public TimeSeriesHandler TimeSeriesHandler { get; }

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
        {
            _dataBase = connectionMultiplexer.GetDatabase();
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
            TimeSeriesHandler = new TimeSeriesHandler(_dataBase.TS());
        }

        public bool ContainsKey(string key)
        {
            return _dataBase.KeyExists(key);
        }

        public void FlushAll()
        {
            _connectionMultiplexer.GetServer(_configuration["Redis:Configuration:ServerAdress"]).FlushDatabase();
        }
    }
}
