using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("versioninfo")]
    public class MeasurementVersionInfo : IBaseMeasurement
    {
        #pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
        #pragma warning restore
        
        [Column("version")] public string Version { get; set; }
        [Column("tag")] public string Tag { get; set; }
        [Column("branch")] public string Branch { get; set; }
        [Column("hash")] public string Hash { get; set; }
        [Column("date")] public string Date { get; set; }
    }
}