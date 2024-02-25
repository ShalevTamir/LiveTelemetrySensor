using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Spire.Additions.Xps.Schema;
using System.Collections.Generic;

namespace LiveTelemetrySensor.Mongo.Models
{
    public class Alerts
    {
        public ObjectId _id {  get; set; }

        [BsonElement("timestamp")]                
        public long TimeStamp { get; set; }

        [BsonElement("alerts")]
        public List<Alert> MongoAlerts { get; set; }
    }
}
