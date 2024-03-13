using LiveTelemetrySensor.Mongo.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Mongo.Services
{
    public class MongoAlertsService
    {
        private readonly IMongoCollection<Models.Alerts> _alertsCollection;
        private const string MONGO_DB_KEY = "MongoDB";
        private const string TIMESTAMP_MONGO_KEY = "timestamp";

        public MongoAlertsService(IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection(MONGO_DB_KEY).Get<AlertsDatabaseSettings>();
            var mongoClient = new MongoClient(databaseSettings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            _alertsCollection = mongoDatabase.GetCollection<Alerts>(databaseSettings.CollectionName);
        }

        public async Task InsertAlerts(Alerts alert) =>
            await _alertsCollection.InsertOneAsync(alert);

        public async Task<long> CountAlerts(long minTimeStamp, long maxTimeStamp)
        {
            FilterDefinition<Models.Alerts> filter = Builders<Models.Alerts>.Filter.And(
                Builders<Models.Alerts>.Filter.Gt(TIMESTAMP_MONGO_KEY, minTimeStamp),
                Builders<Models.Alerts>.Filter.Lt(TIMESTAMP_MONGO_KEY, maxTimeStamp)
                );
            return await _alertsCollection.CountDocumentsAsync(filter);
        }

        public async Task<List<Alerts>> GetAlerts(long minTimeSpan, long maxTimeSpan, int maxSamplesInPage, int pageNumber)
        {
            var findOptions = new FindOptions<Models.Alerts>
            {
                Limit = maxSamplesInPage,
                Skip = (pageNumber) * maxSamplesInPage,
                //Sort = Builders<Alert>.Sort.Ascending(TIMESTAMP_MONGO_KEY)
            };
            FilterDefinition<Models.Alerts> filter = Builders<Models.Alerts>.Filter.And(
                Builders<Models.Alerts>.Filter.Gt(TIMESTAMP_MONGO_KEY, minTimeSpan),
                Builders<Models.Alerts>.Filter.Lt(TIMESTAMP_MONGO_KEY, maxTimeSpan)
                );
            using (IAsyncCursor<Models.Alerts> cursor = await _alertsCollection.FindAsync(filter, findOptions))
            {
                return cursor.ToList();
            }
        }
    }
}
