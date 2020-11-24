using Optional;

namespace dck_pihole2influx.StatObjects
{
    public interface IBaseResult
    {
    }

    public sealed record PrimitiveResultString(string Value):IBaseResult;
    
    public sealed record PrimitiveResultInt(int Value):IBaseResult;

    public sealed record PrimitiveResultLong(long Value):IBaseResult;

    public sealed record PrimitiveResultFloat(float Value):IBaseResult;

    public sealed record StringDecimalValue(string Key, decimal Value):IBaseResult;

    public sealed record IntOutputNumberedElement(int Count, int Position, string IpOrHost):IBaseResult;

    public sealed record DoubleStringOutputElement(int Count, int Position, string IpAddress, Option<string> HostName):IBaseResult;

    public sealed record OvertimeOutputElement(long TimeStamp, int PermitValue, int BlockValue):IBaseResult;

    public sealed record DoubleOutputNumberedElement(double Count, int Position, string IpOrHost):IBaseResult;
}