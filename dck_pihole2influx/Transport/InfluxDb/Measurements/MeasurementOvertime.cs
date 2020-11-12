using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("overtime")]
    public class MeasurementOvertime : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;

        [Column("permitValue")] public int PermitValue { get; set; }
        
        [Column("blockValue")] public int BlockValue { get; set; }

    }
}