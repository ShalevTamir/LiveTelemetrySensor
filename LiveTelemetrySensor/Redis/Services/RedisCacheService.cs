using LiveTelemetrySensor.Redis.Interfaces;
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
        public TimeSeriesHandler TimeSeriesHandler { get; }

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _dataBase = connectionMultiplexer.GetDatabase();
            TimeSeriesHandler = new TimeSeriesHandler(_dataBase.TS());
        }
    }
}
