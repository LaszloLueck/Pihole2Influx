namespace dck_pihole2influx.StatObjects
{
    public class PatternValue
    {
        public readonly string GivenName;
        public readonly ValueTypes ValueType;
        public readonly dynamic AlternativeValue;

        public PatternValue(string givenName, ValueTypes valueType, dynamic alternativeValue)
        {
            GivenName = givenName;
            ValueType = valueType;
            AlternativeValue = alternativeValue;
        }
    }
}