using System;
using InfluxDB.Client.Core;

namespace dck_pihole2influx.Transport.InfluxDb.Measurements
{
    [Measurement("stats")]
    public class MeasurementStats : IBaseMeasurement
    {
        [Column(IsTimestamp = true)] public DateTime Time;
        [Column("domainsBeingBlocked")] public int DomainsBeingBlocked { get; set; }
        [Column("dnsQueriesToday")] public int DnsQueriesToday { get; set; }
        [Column("adsBlockedToday")] public int AdsBlockedToday { get; set; }
        [Column("adsPercentageToday")] public float AdsPercentageToday { get; set; }
        [Column("uniqueDomains")] public int UniqueDomains { get; set; }
        [Column("queriesForwarded")] public int QueriesForwarded { get; set; }
        [Column("queriesCached")] public int QueriesCached { get; set; }
        [Column("clientsEverSeen")] public int ClientsEverSeen { get; set; }
        [Column("uniqueClients")] public int UniqueClients { get; set; }
        [Column("status")] public string Status { get; set; }
    }
}