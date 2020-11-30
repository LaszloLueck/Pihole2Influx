using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("telnetReadError")]
    public class MeasurementTelnetReadError : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        
        
    }
}