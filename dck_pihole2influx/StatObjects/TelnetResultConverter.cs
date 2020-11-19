using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    public abstract class TelnetResultConverter : ConverterUtils
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<TelnetResultConverter>.GetLogger();
        private string _input;


        public Option<ConcurrentDictionary<string, IBaseResult>> DictionaryOpt { get; private set; }

        public async Task Convert(string input)
        {
            _input = input;
            DictionaryOpt = await GetDtoFromResult();
        }

        public abstract PiholeCommands GetPiholeCommand();

        public abstract Task<List<IBaseMeasurement>> CalculateMeasurementData();

        public abstract Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint);

        protected abstract Option<(string, IBaseResult)> CalculateTupleFromString(string line);

        public virtual string GetTerminator()
        {
            return "---EOM---";
        }


        private async Task<Option<ConcurrentDictionary<string, IBaseResult>>> GetDtoFromResult()
        {
            if (_input.Length == 0)
            {
                await Log.WarningAsync("the input string (telnet result) contains no data, please check your configuration.");
                return Option.None<ConcurrentDictionary<string, IBaseResult>>();
            }

            try
            {
                var ret = new ConcurrentDictionary<string, IBaseResult>();
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

                return !ret.IsEmpty ? Option.Some(ret) : Option.None<ConcurrentDictionary<string, IBaseResult>>();
            }
            catch (Exception ex)
            {
                await Log.ErrorAsync(ex, "Error while create an object from return string");
                await Log.WarningAsync(_input);
                return Option.None<ConcurrentDictionary<string, IBaseResult>>();
            }
        }
    }
}