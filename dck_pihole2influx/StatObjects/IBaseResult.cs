namespace dck_pihole2influx.StatObjects
{
    public interface IBaseResult
    {
    }

    public class PrimitiveResultString : IBaseResult
    {
        public readonly string _value;

        public PrimitiveResultString(string value)
        {
            _value = value;
        }
    }

    public class PrimitiveResultInt : IBaseResult
    {
        public readonly int _value;

        public PrimitiveResultInt(int value)
        {
            _value = value;
        }
    }

    public class PrimitiveResultFloat : IBaseResult
    {
        public readonly float _value;

        public PrimitiveResultFloat(float value)
        {
            _value = value;
        }
    }

    public class StringDoubleOutput : IBaseResult
    {
        public readonly string Key;
        public readonly double Value;

        public StringDoubleOutput(string key, double value)
        {
            Key = key;
            Value = value;
        }
    }

    public class IntOutputNumberedList : IBaseResult
    {
        public readonly int Count;
        public readonly string Position;
        public readonly string IpOrHost;

        public IntOutputNumberedList(int count, string position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }

    public class DoubleOutputNumberedList : IBaseResult
    {
        public readonly double Count;
        public readonly string Position;
        public readonly string IpOrHost;

        public DoubleOutputNumberedList(double count, string position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }
}