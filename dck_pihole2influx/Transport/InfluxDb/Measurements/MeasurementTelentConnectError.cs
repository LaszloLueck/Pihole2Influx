using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("telnetConnectError")]
    public class MeasurementTelnetConnectError : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        
        
        
    }
}