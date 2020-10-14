using Optional;

namespace dck_pihole2influx.StatObjects
{
    public interface IBaseResult
    {
    }

    public class PrimitiveResultString : IBaseResult
    {
        public string Value { get; }

        public PrimitiveResultString(string value)
        {
            Value = value;
        }
    }

    public class PrimitiveResultInt : IBaseResult
    {
        public int Value { get; }

        public PrimitiveResultInt(int value)
        {
            Value = value;
        }
    }

    public class PrimitiveResultFloat : IBaseResult
    {
        public float Value { get; }

        public PrimitiveResultFloat(float value)
        {
            Value = value;
        }
    }

    public class StringDoubleOutput : IBaseResult
    {
        public string Key { get; }
        public double Value { get; }

        public StringDoubleOutput(string key, double value)
        {
            Key = key;
            Value = value;
        }
    }

    public class IntOutputNumberedList : IBaseResult
    {
        public int Count { get; }
        public string Position { get; }
        public string IpOrHost { get; }

        public IntOutputNumberedList(int count, string position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }

    public class DoubleStringOutputList : IBaseResult
    {
        public int Count { get; }
        public int Position { get; }
        public string IpAddress { get; }
        public Option<string> HostName { get; }

        public DoubleStringOutputList(int position, int count, string ipAddress, Option<string> hostName)
        {
            Position = position;
            Count = count;
            IpAddress = ipAddress;
            HostName = hostName;
        }
    }

    public class DoubleOutputNumberedList : IBaseResult
    {
        public double Count { get; }
        public string Position { get; }
        public string IpOrHost { get; }

        public DoubleOutputNumberedList(double count, string position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }
}