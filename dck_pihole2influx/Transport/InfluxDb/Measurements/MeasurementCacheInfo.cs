using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("cacheinfo")]
    public class MeasurementCacheInfo : IBaseMeasurement
    {
        #pragma warning disable
        [Column(IsTimestamp = true)] public DateTime Time;
        #pragma warning restore
        [Column("cacheSize")] public int CacheSize { get; set; }
        [Column("cacheLiveFreed")] public long CacheLiveFreed { get; set; }
        [Column("cacheInserted")] public long CacheInserted { get; set; }
    }
}