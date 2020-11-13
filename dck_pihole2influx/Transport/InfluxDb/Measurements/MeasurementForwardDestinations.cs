using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("forwardDestinations")]
    public class MeasurementForwardDestinations : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        [Column("percentage")] public double Percentage { get; set; }
        [Column("position")] public int Position { get; set; }
        [Column("ipOrHostName", IsTag = true)] public string IpOrHostName { get; set; }
    }
}