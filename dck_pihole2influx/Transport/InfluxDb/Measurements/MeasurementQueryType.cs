using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("querytype")]
    public class MeasurementQueryType : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;
        
        [Column("dnsType", IsTag = true)] public string DnsType { get; set; }
        
        [Column("value")] public decimal Value { get; set; }
    }
}