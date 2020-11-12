using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class DbStatsConverterTest : TestHelperUtils
    {

        private readonly TelnetResultConverter _telnetResultConverter;

        public DbStatsConverterTest()
        {
            _telnetResultConverter = new DbStatsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "queries in database: 4934790\ndatabase filesize: 393.86 MB\nSQLite version: 3.31.1\n\n---EOM---";
            _telnetResultConverter.Convert(testee).Wait();
            var expectedDictionary = new Dictionary<string, IBaseResult>
            {
                {DbStatsConverter.DatabaseFileSize, new PrimitiveResultString("393.86 MB")},
                {DbStatsConverter.QueriesInDatabase, new PrimitiveResultLong(4934790)},
                {DbStatsConverter.SqLiteVersion, new PrimitiveResultString("3.31.1")}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(expectedDictionary);

            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Dbstats.ToString());

            var jsonExpected = "{\"DatabaseFileSize\":\"393.86 MB\",\"QueriesInDatabase\":4934790,\"SqLiteVersion\":\"3.31.1\"}";
            var orderedExpectedJson = OrderJsonObjectString(jsonExpected).ValueOr("");

            var orderedCurrentJson =
                OrderJsonObjectString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result)
                    .ValueOr("");

            orderedCurrentJson.Should().NotBeEmpty();
            orderedExpectedJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);

        }


    }
}