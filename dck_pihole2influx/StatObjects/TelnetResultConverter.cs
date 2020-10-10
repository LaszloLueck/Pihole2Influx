using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.Telnet;
using Optional;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    public abstract class TelnetResultConverter : ConverterUtils
    {
        private static readonly ILogger Log = LoggingFactory<TelnetResultConverter>.CreateLogging();
        private string _input;

        public Option<ConcurrentDictionary<string, dynamic>> DictionaryOpt { get; private set; }

        public async Task Convert(string input)
        {
            _input = input;
            DictionaryOpt = await GetDtoFromResult();
        }

        public abstract PiholeCommands GetPiholeCommand();

        public abstract Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint);

        protected abstract Option<(string, dynamic)> CalculateTupleFromString(string line);

        public string GetTerminator()
        {
            return "---EOM---";
        }


        private async Task<Option<ConcurrentDictionary<string, dynamic>>> GetDtoFromResult()
        {
            if (_input.Length == 0)
            {
                Log.Warning("the input string (telnet result) contains no data, please check your configuration.");
                return Option.None<ConcurrentDictionary<string, dynamic>>();
            }

            try
            {
                var ret = new ConcurrentDictionary<string, dynamic>();
                var tasks = new ConcurrentBag<Task>();

                SplitInputString(_input, GetTerminator()).MatchSome(splitted =>
                {
                    foreach (var s in splitted)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            CalculateTupleFromString(s).MatchSome(result => ret.TryAdd(result.Item1, result.Item2));
                        }));
                    }
                });

                await Task.WhenAll(tasks);

                return ret.Count > 0 ? Option.Some(ret) : Option.None<ConcurrentDictionary<string, dynamic>>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while create an object from return string");
                Log.Warning(_input);
                return Option.None<ConcurrentDictionary<string, dynamic>>();
            }
        }
    }
}