using System.Collections.Generic;
using dck_pihole2influx.Transport.Telnet;

namespace dck_pihole2influx.StatObjects
{
    
    /// <summary>
    /// Topads contains a list of top blocked daomains with the amount
    /// 0 8 googleads.g.doubleclick.net
    /// 1 6 www.googleadservices.com
    /// 2 1 cdn.mxpnl.com
    /// 3 1 collector.githubapp.com
    /// 4 1 www.googletagmanager.com
    /// 5 1 s.zkcdn.net
    /// </summary>
    public class TopAdsConverter : TelnetResultConverter
    {
        protected override Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Topads;
        }

        public override ConverterType GetConverterType()
        {
            return ConverterType.NumberedUrlList;
        }
    }
}