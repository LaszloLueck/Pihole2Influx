using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("forwardDestinations")]
    public class MeasurementForwardDestinations : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;
        [Column("percentage")] public double Percentage { get; set; }
        [Column("position")] public int Position { get; set; }
        [Column("ipOrHostName")] public string IpOrHostName { get; set; }
    }
}