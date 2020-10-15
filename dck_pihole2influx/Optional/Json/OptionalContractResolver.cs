using System;
using Newtonsoft.Json.Serialization;
using Optional;

namespace dck_pihole2influx.Optional.Json
{
    public class OptionalContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType.Assembly == typeof(Option).Assembly)
            {
                contract.Converter = new OptionConverter();
            }

            return contract;
        }
    }
}