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

    public class PrimitiveResultLong : IBaseResult
    {
        public long Value { get; }

        public PrimitiveResultLong(long value)
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

    public class StringDecimalOutput : IBaseResult
    {
        public string Key { get; }
        public decimal Value { get; }

        public StringDecimalOutput(string key, decimal val)
        {
            Key = key;
            Value = val;
        }
    }

    public class IntOutputNumberedElement : IBaseResult
    {
        public int Count { get; }
        public int Position { get; }
        public string IpOrHost { get; }

        public IntOutputNumberedElement(int count, int position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }

    public class DoubleStringOutputElement : IBaseResult
    {
        public int Count { get; }
        public int Position { get; }
        public string IpAddress { get; }
        public Option<string> HostName { get; }

        public DoubleStringOutputElement(int position, int count, string ipAddress, Option<string> hostName)
        {
            Position = position;
            Count = count;
            IpAddress = ipAddress;
            HostName = hostName;
        }
    }

    public class OvertimeOutputElement : IBaseResult
    {
        public long TimeStamp { get; }
        public int PermitValue { get; }
        public int BlockValue { get; }
        
        public OvertimeOutputElement(long timeStamp, int permitValue, int blockValue)
        {
            TimeStamp = timeStamp;
            PermitValue = permitValue;
            BlockValue = blockValue;
        }
    }

    public class DoubleOutputNumberedElement : IBaseResult
    {
        public double Count { get; }
        public int Position { get; }
        public string IpOrHost { get; }

        public DoubleOutputNumberedElement(double count, int position, string ipOrHost)
        {
            Count = count;
            Position = position;
            IpOrHost = ipOrHost;
        }
    }
}