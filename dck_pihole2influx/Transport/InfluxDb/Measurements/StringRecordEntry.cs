namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    public class StringRecordEntry : IBaseMeasurement
    {
        public string Key;
        public string Value;
    }
}