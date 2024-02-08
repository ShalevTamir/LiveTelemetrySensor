using LiveTelemetrySensor.Mongo.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Mongo.Services
{
    public class MongoAlertsService
    {
        private readonly IMongoCollection<Alert> _alertsCollection;
        private const string MONGO_DB_KEY = "MongoDB";
        private const string TIMESTAMP_MONGO_KEY = "timestamp";

        public MongoAlertsService(IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection(MONGO_DB_KEY).Get<AlertsDatabaseSettings>();
            var mongoClient = new MongoClient(databaseSettings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            _alertsCollection = mongoDatabase.GetCollection<Alert>(databaseSettings.CollectionName);
        }

        public async Task InsertAlert(Alert alert) =>
            await _alertsCollection.InsertOneAsync(alert);

        public async Task<long> CountAlerts(long minTimeStamp, long maxTimeStamp)
        {
            FilterDefinition<Alert> filter = Builders<Alert>.Filter.And(
                Builders<Alert>.Filter.Gt(TIMESTAMP_MONGO_KEY, minTimeStamp),
                Builders<Alert>.Filter.Lt(TIMESTAMP_MONGO_KEY, maxTimeStamp)
                );
            return await _alertsCollection.CountDocumentsAsync(filter);
        }

        public async Task<List<Alert>> GetAlerts(long minTimeSpan, long maxTimeSpan, int maxSamplesInPage, int pageNumber)
        {
            var findOptions = new FindOptions<Alert>
            {
                Limit = maxSamplesInPage,
                Skip = (pageNumber) * maxSamplesInPage,
                //Sort = Builders<Alert>.Sort.Ascending(TIMESTAMP_MONGO_KEY)
            };
            FilterDefinition<Alert> filter = Builders<Alert>.Filter.And(
                Builders<Alert>.Filter.Gt(TIMESTAMP_MONGO_KEY, minTimeSpan),
                Builders<Alert>.Filter.Lt(TIMESTAMP_MONGO_KEY, maxTimeSpan)
                );
            using (IAsyncCursor<Alert> cursor = await _alertsCollection.FindAsync(filter, findOptions))
            {
                return cursor.ToList();
            }
        }
    }
}
