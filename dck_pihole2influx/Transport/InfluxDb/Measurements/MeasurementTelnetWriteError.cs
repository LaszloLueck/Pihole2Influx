using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("telnetWriteError")]
    public class MeasurementTelnetWriteError : IBaseMeasurement
    {
#pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
#pragma warning restore
        [Column("writeErrorType")] public string WriteErrorType { get; set; } 
        
        
    }
}