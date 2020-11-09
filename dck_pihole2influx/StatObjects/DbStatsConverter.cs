using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// DbStats receives informations about ftls internal sqlite database. The resultset looks like:
    /// >dbstats
    ///queries in database: 4934790
    ///database filesize: 393.86 MB
    ///SQLite version: 3.31.1
    /// </summary>

    public class DbStatsConverter : TelnetResultConverter, IBaseConverter
    {
        public const string QueriesInDatabase = "QueriesInDatabase";
        public const string DatabaseFileSize = "DatabaseFileSize";
        public const string SqLiteVersion = "SqLiteVersion";
        
        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Dbstats;
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

        public Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>
        {
            {"queries in database:", new PatternValue(QueriesInDatabase, ValueTypes.Long, 0)},
            {"database filesize:", new PatternValue(DatabaseFileSize, ValueTypes.String, "")},
            {"SQLite version:", new PatternValue(SqLiteVersion, ValueTypes.String, "")}
        };
    }
}