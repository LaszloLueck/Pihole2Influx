using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("telnetError")]
    public class MeasurementTelnetError : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        [Column("errorType", IsTag = true)] public string ErrorType { get; set; }
        
        [Column("onMethod")] public string OnMethod { get; set; }
        
        
    }
}