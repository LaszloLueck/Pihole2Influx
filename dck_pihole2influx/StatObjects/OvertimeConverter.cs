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
    /// Overtime contains a list of (collected per 10 minutes) permitted / blocked count urls.
    ///1603127100 444 21
    ///1603127700 636 99
    ///1603128300 888 58
    ///1603128900 917 33
    ///1603129500 400 15
    ///1603130100 1329 77
    ///1603130700 1057 99
    ///1603131300 771 100
    ///1603131900 1158 119
    ///1603132500 1658 57
    ///---EOM---
    /// </summary>
    public class OvertimeConverter : TelnetResultConverter, IBaseConverter
    {
        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Overtime;
        }

        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                    {
                        return (from tuple in dic select tuple).Select(tuple =>
                        {
                            var convValue = (OvertimeOutputElement) tuple.Value;
                            var convertedDateTime = DateTimeOffset.FromUnixTimeSeconds(convValue.TimeStamp).DateTime;
                            return new MeasurementOvertime
                            {
                                BlockValue = convValue.BlockValue, PermitValue = convValue.PermitValue,
                                Time = convertedDateTime.Add(TimeSpan.FromSeconds(2))
                            };
                        });
                    })
                    .ValueOr(new List<MeasurementOvertime>())
                    .OrderBy(e => e.Time)
                    .Select(element => (IBaseMeasurement) element)
                    .ToList();
            });
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .OrderBy(element => element.Key)
                .Select(element => (OvertimeOutputElement) element.Value);

            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForOvertime(line, GetPattern());
        }

        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }
    }
}