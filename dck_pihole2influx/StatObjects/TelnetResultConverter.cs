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
                            case ConverterType.ColonSplit:
                            {
                                ConverterUtils.ConvertColonSplittedLine(s, GetPattern())
                                    .MatchSome(result => ret.TryAdd(result.Item1, result.Item2));
                                break;
                            }
                            case ConverterType.NumberedPercentageList:
                            {
                                ConverterUtils.ConvertResultForNumberedPercentage(s, GetPattern())
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

                return ret.Count > 0 ? Option.Some(ret) : Option.None<ConcurrentDictionary<string, dynamic>>();
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
                    return await ConvertOutputToJson(obj, prettyPrint);
                }
                case ConverterType.NumberedUrlList:
                {
                    var to = (from element in obj select GetNumberdUrlFromKeyValue(element))
                        .OrderBy(element => element.position);

                    return await ConvertOutputToJson(to, prettyPrint);
                }
                case ConverterType.ColonSplit:
                {
                    return await ConvertOutputToJson(obj, prettyPrint);
                }
                case ConverterType.NumberedPercentageList:
                {
                    var to = (from element in obj select GetNumberedPercentageFromKeyValue(element)).OrderBy(element =>
                        element.position);
                    return await ConvertOutputToJson(to, prettyPrint);
                }
                default:
                    Log.Warning(
                        "Unidentified / Unprocessable ConverterType used. Please implement a processing for this type");
                    return await Task.Run(() => string.Empty);
            }
        }

        private static async Task<string> ConvertOutputToJson<T>(T output, bool prettyPrint)
        {
            try
            {
                await using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, output, output.GetType(),
                    new JsonSerializerOptions() {WriteIndented = prettyPrint});
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while processing a data structure to json string");
                return await Task.Run(() => string.Empty);
            }
        }

        private NumberedUrlItem GetNumberdUrlFromKeyValue(KeyValuePair<string, dynamic> keyValue)
        {
            int key = int.TryParse(keyValue.Key, out key) ? key : 0;
            var tpl = ((int, string)) keyValue.Value;
            return new NumberedUrlItem(key, tpl.Item1, tpl.Item2);
        }

        private NumberedPercentageItem GetNumberedPercentageFromKeyValue(KeyValuePair<string, dynamic> keyvalue)
        {
            int key = int.TryParse(keyvalue.Key, out key) ? key : 0;
            var tpl = ((double, string)) keyvalue.Value;
            return new NumberedPercentageItem(key, tpl.Item1, tpl.Item2);
        }
    }
}