namespace LiveTelemetrySensor.Mongo.Models
{
    public class AlertsDatabaseSettings
    {
        public string? ConnectionString { get; set; } = null;
        public string? DatabaseName { get; set; } = null;
        public string? CollectionName { get; set; } = null;
    }
}
