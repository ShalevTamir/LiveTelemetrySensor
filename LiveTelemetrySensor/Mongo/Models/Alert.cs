using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LiveTelemetrySensor.Mongo.Models
{
    public class Alert
    {
        public ObjectId _id {  get; set; }

        [BsonElement("timestamp")]                
        public long TimeStamp { get; set; }

        [BsonElement("sensorName")]
        public string SensorName { get; set; }

        [BsonElement("sensorStatus")]
        public SensorState SensorStatus { get; set; }
    }
}
