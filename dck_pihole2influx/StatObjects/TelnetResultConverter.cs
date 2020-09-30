using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.Telnet;
using Optional;
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

        public virtual ConverterType GetConverterType()
        {
            return ConverterType.Standard;
        }

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
                        switch (GetConverterType())
                        {
                            case ConverterType.Standard:
                            {
                                ConverterUtils.ConvertResultForStandard(s, GetPattern())
                                    .MatchSome(result => ret.TryAdd(result.Item1, result.Item2));
                                break;
                            }
                            case ConverterType.NumberedUrlList:
                            {
                                ConverterUtils.ConvertResultForNumberedUrlList(s, GetPattern())
                                    .MatchSome(result => ret.TryAdd(result.Item1, result.Item2));
                                break;
                            }
                            default:
                                Log.Warning(
                                    "Unidentified / Unprocessable ConverterType used. Please implement a processing for this type");
                                break;
                        }
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
            switch (GetConverterType())
            {
                case ConverterType.Standard:
                {
                    await using var stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, obj, obj.GetType(),
                        new JsonSerializerOptions() {WriteIndented = prettyPrint});
                    stream.Position = 0;
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
                case ConverterType.NumberedUrlList:
                    return await Task.Run(() => string.Empty);
                default:
                    Log.Warning(
                        "Unidentified / Unprocessable ConverterType used. Please implement a processing for this type");
                    return await Task.Run(() => string.Empty);
            }
        }
    }
}