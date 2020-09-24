﻿using System;
using dck_pihole2influx.Logging;
using Optional;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    public interface IBaseX
    {
    }

    public class BaseValue<T> : IBaseX
    {
        private readonly T _value;

        public T GetValue()
        {
            return _value;
        }

        public BaseValue(T value)
        {
            _value = value;
        }
    }

    public class ValueConverterBase<T>
    {
        private static readonly ILogger Log = LoggingFactory<ValueConverterBase<T>>.CreateLogging();

        private static string RemoveKeyAndTrim(string key, string input)
        {
            return input.Replace(key, "").TrimStart().TrimEnd();
        }

        public static Option<IBaseX> Convert(string input, string key, T alternative)
        {
            switch (alternative)
            {
                case int i:
                    int parsedValue = int.TryParse(RemoveKeyAndTrim(key, input), out parsedValue)
                        ? parsedValue
                        : i;
                    IBaseX retInt = new BaseValue<int>(parsedValue);

                    return Option.Some(retInt);
                case string s:
                    try
                    {
                        IBaseX retString = new BaseValue<string>(RemoveKeyAndTrim(key, input));
                        return Option.Some(retString);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occured!");
                    }

                    return Option.None<IBaseX>();
                default:
                    Log.Warning($"An inconvertible type found and get back an None. Type is {typeof(T).FullName}");
                    return Option.None<IBaseX>();
            }
        }
    }
}