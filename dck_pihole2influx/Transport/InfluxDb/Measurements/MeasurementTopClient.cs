using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("topclient")]
    public class MeasurementTopClient : IBaseMeasurement
    {
        [Column("clientIp")] public string ClientIp { get; set; }
        [Column("position", IsTag = true)] public int Position { get; set; }
        [Column("hostName")] public string HostName { get; set; }
        [Column("count")] public int Count { get; set; }
        [Column(IsTimestamp = true)] public DateTime Time { get; set; }
    }
}