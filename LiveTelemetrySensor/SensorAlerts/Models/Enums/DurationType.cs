namespace LiveTelemetrySensor.SensorAlerts.Models.Enums
{
    // Each DurationType has it's corresponding milliseconds multipler as it's value
    public enum DurationType 
    {
        SECONDS = 1000,
        MINUTES = 60_000,
        HOURS = 3_600_000
    }
}
