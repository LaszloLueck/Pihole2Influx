using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("dbstats")]
    public class MeasurementDbStats : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;
        [Column("databaseVersion")] public string DatabaseVersion { get; set; }
        [Column("databaseFileSize")] public double DatabaseFileSize { get; set; }
        [Column("databaseEntries")] public long DatabaseEntries { get; set; }
    }
}