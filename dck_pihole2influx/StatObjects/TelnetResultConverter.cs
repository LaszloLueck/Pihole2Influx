using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.Telnet;
using Optional;
using Optional.Collections;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    public abstract class TelnetResultConverter
    {
        private static readonly ILogger Log = LoggingFactory<TelnetResultConverter>.CreateLogging();
        private string _input;

        public Option<ConcurrentDictionary<string, dynamic>> DictionaryOpt { get; private set; }

        public async Task Convert(string input)
        {
            _input = input;
            DictionaryOpt = await GetDtoFromResult();
        }

        protected abstract Dictionary<string, PatternValue> GetPattern();

        public abstract PiholeCommands GetPiholeCommand();

        public virtual string GetTerminator()
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

            var splitted = _input
                .Split("\n")
                .Where(s => !string.IsNullOrWhiteSpace(s) && s != GetTerminator())
                .AsParallel()
                .AsOrdered();

            try
            {
                var ret = new ConcurrentDictionary<string, dynamic>();
                var tasks = new ConcurrentBag<Task>();


                foreach (var s in splitted)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        GetPattern().FirstOrNone(value => s.Contains(value.Key)).Map(
                            result =>
                            {
                                var (key, patternValue) = result;
                                return patternValue.ValueType switch
                                {
                                    ValueTypes.Int => ValueConverterBase<int>
                                        .Convert(s, key, (int) patternValue.AlternativeValue)
                                        .Map<(string, dynamic)>(
                                            value => (patternValue.GivenName, ((BaseValue<int>) value).GetValue())),
                                    ValueTypes.String => ValueConverterBase<string>
                                        .Convert(s, key, (string) patternValue.AlternativeValue)
                                        .Map<(string, dynamic)>(value =>
                                            (patternValue.GivenName, ((BaseValue<string>) value).GetValue())),
                                    ValueTypes.Float => ValueConverterBase<float>
                                        .Convert(s, key, (float) patternValue.AlternativeValue)
                                        .Map<(string, dynamic)>(value =>
                                            (patternValue.GivenName, ((BaseValue<float>) value).GetValue())),
                                    _ => Option.None<(string, dynamic)>()
                                };
                            }).Flatten().MatchSome(tuple => ret.TryAdd(tuple.Item1, tuple.Item2));
                    }));
                }

                await Task.WhenAll(tasks);

                if (ret.Count == GetPattern().Count) return Option.Some(ret);

                Log.Warning(
                    $"The results contains less ({ret.Count} entries) data than the configuration ({GetPattern().Count} entries)");
                return Option.None<ConcurrentDictionary<string, object>>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while create an object from return string");
                Log.Warning(_input);
                return Option.None<ConcurrentDictionary<string, dynamic>>();
            }
        }

        public async Task<string> GetJsonFromObjectAsync(bool prettyPrint = false)
        {
            var obj = DictionaryOpt.Match(
                some: dic => dic,
                none: () =>
                {
                    Log.Warning("Cannot convert dictionary to json, dictionary is none!");
                    return new ConcurrentDictionary<string, dynamic>();
                });
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, obj.GetType(),
                new JsonSerializerOptions() {WriteIndented = prettyPrint});
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}