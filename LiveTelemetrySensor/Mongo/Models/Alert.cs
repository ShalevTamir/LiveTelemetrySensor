using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace LiveTelemetrySensor.Mongo.Models
{
    public class Alert
    {
        [BsonElement("sensorName")]
        public string SensorName { get; set; }

        [BsonElement("sensorStatus")]
        public SensorState SensorStatus { get; set; }
    }
}
