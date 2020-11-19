using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// >version
    ///version v5.2
    ///tag v5.2
    ///branch master
    ///hash dbd4a69
    ///date 2020-08-09 22:09:43 +0100
    ///---EOM---


    /// </summary>
    
    public class VersionInfoConverter : TelnetResultConverter, IBaseConverter
    {
        public const string Version = "Version";
        public const string Tag = "Tag";
        public const string Branch = "Branch";
        public const string Hash = "Hash";
        public const string Date = "Date";

        public Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>()
        {
            {"version", new PatternValue(Version, ValueTypes.String, "")},
            {"tag", new PatternValue(Tag, ValueTypes.String, "")},
            {"branch", new PatternValue(Branch, ValueTypes.String, "")},
            {"hash", new PatternValue(Hash, ValueTypes.String, "")},
            {"date", new PatternValue(Date, ValueTypes.String, "")}
        };

        
        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                {
                    var version = ((PrimitiveResultString) dic[Version]).Value;
                    var tag = ((PrimitiveResultString) dic[Tag]).Value;
                    var branch = ((PrimitiveResultString) dic[Branch]).Value;
                    var hash = ((PrimitiveResultString) dic[Hash]).Value;
                    var date = ((PrimitiveResultString) dic[Date]).Value;

                    var returnValue = new MeasurementVersionInfo()
                    {
                        Branch = branch,
                        Date = date,
                        Hash = hash,
                        Tag = tag,
                        Time = DateTime.Now,
                        Version = version
                    };
                    return new List<IBaseMeasurement>() {returnValue};

                }).ValueOr(new List<IBaseMeasurement>());
            });
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Version;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .Select(ConvertIBaseResultToPrimitive)
                .ToDictionary(element => element.Item1, element => element.Item2);
            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForStandard(line, GetPattern());
        }
    }
}