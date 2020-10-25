using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class VersionInfoConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public VersionInfoConverterTest()
        {
            _telnetResultConverter = new VersionInfoConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"version v5.2
tag v5.2
branch master
hash dbd4a69
date 2020-08-09 22:09:43 +0100
---EOM---";

            _telnetResultConverter.Convert(testee).Wait();
            var dictionaryExpected = new Dictionary<string, IBaseResult>
            {
                {VersionInfoConverter.Version, new PrimitiveResultString("v5.2")},
                {VersionInfoConverter.Branch, new PrimitiveResultString("master")},
                {VersionInfoConverter.Date, new PrimitiveResultString("2020-08-09 22:09:43 +0100")},
                {VersionInfoConverter.Hash, new PrimitiveResultString("dbd4a69")},
                {VersionInfoConverter.Tag, new PrimitiveResultString("v5.2")}
            }.OrderBy(element => element.Key);

            var dictionaryResult =
                _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>())
                    .OrderBy(element => element.Key);

            dictionaryResult.Should().BeEquivalentTo(dictionaryExpected);

            var jsonExpected =
                "{\"Tag\":\"v5.2\",\"Hash\":\"dbd4a69\",\"Branch\":\"master\",\"Version\":\"v5.2\",\"Date\":\"2020-08-09 22:09:43 +0100\"}";
            var orderedJsonExpected = OrderJsonObjectStringByName(jsonExpected).ValueOr("");

            var orderedJsonResult =
                OrderJsonObjectStringByName(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result).ValueOr("");

            orderedJsonExpected.Should().NotBeEmpty();
            orderedJsonResult.Should().NotBeEmpty();
            orderedJsonExpected.Should().Be(orderedJsonResult);
        }
    }
}