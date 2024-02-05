using LiveTelemetrySensor.Mongo.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Mongo.Services
{
    public class MongoAlertsService
    {
        private readonly IMongoCollection<Alert> _alertsCollection;
        private const string MONGO_DB_KEY = "MongoDB";
        
        public MongoAlertsService(IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection(MONGO_DB_KEY).Get<AlertsDatabaseSettings>();
            var mongoClient = new MongoClient(databaseSettings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            _alertsCollection = mongoDatabase.GetCollection<Alert>(databaseSettings.CollectionName);
        }

        public async Task InsertAlert(Alert alert) =>
            await _alertsCollection.InsertOneAsync(alert);
    }
}
