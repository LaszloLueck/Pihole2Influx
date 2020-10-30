using System;
using System.Globalization;
using dck_pihole2influx.Logging;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    public interface IBaseValue
    {
    }

    public class BaseValue<T> : IBaseValue
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
        protected ValueConverterBase()
        {
            
        }
        
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<ValueConverterBase<T>>.GetLogger();

        private static string RemoveKeyAndTrim(string key, string input)
        {
            return input.Replace(key, "").TrimStart().TrimEnd();
        }

        public static Option<IBaseValue> Convert(string input, string key, T alternative)
        {
            switch (alternative)
            {
                case int i:
                    return int.TryParse(RemoveKeyAndTrim(key, input), out var intValue)
                        ? Option.Some((IBaseValue)new BaseValue<int>(intValue))
                        : Option.Some((IBaseValue)new BaseValue<int>(i));
                case string s:
                    try
                    {
                        IBaseValue retString = new BaseValue<string>(RemoveKeyAndTrim(key, input));
                        return Option.Some(retString);
                    }
                    catch (Exception ex)
                    {
                        IBaseValue retString = new BaseValue<string>(s);
                        Log.Error(ex, "An error occured while converting a text value!");
                        return Option.Some(retString);
                    }
                case float f:
                    return float.TryParse(RemoveKeyAndTrim(key, input), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var floatValue)
                        ? Option.Some((IBaseValue) new BaseValue<float>(floatValue))
                        : Option.Some((IBaseValue) new BaseValue<float>(f));
                case long l:
                    return long.TryParse(RemoveKeyAndTrim(key, input), out var longValue)
                        ? Option.Some((IBaseValue) new BaseValue<long>(longValue))
                        : Option.Some((IBaseValue) new BaseValue<long>(l));
                default:
                    Log.Warning($"An inconvertible type found and get back an None. Type is {typeof(T).FullName}");
                    return Option.None<IBaseValue>();
            }
        }
    } 
}