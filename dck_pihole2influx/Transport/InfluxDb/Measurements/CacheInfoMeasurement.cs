using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("cacheinfo")]
    public class CacheInfoMeasurement : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;
        [Column("cacheSize")] public int CacheSize { get; set; }
        [Column("cacheLiveFreed")] public long CacheLiveFreed { get; set; }
        [Column("cacheInserted")] public long CacheInserted { get; set; }
    }
}