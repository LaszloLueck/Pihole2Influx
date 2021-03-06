using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("topads")]
    public class MeasurementTopAds : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        
        [Column("count")] public int Count { get; set; }
        
        [Column("position", IsTag = true)] public int Position { get; set; }
        
        [Column("iporhost")] public string IpOrHost { get; set; }
    }
}