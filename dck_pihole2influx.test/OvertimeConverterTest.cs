using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class OvertimeConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public OvertimeConverterTest()
        {
            _telnetResultConverter = new OvertimeConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"1603127100 444 21
1603127700 636 99
1603128300 888 58
1603128900 917 33
1603129500 400 15
1603130100 1329 77
1603130700 1057 99
1603131300 771 100
1603131900 1158 119
1603132500 1658 57


---EOM---";

            _telnetResultConverter.Convert(testee).Wait();
            var dictionaryExpected = new Dictionary<string, OvertimeOutputElement>
            {
                {"1603127100", new OvertimeOutputElement(1603127100L, 444, 22)},
                {"1603127700", new OvertimeOutputElement(1603127700L, 636, 99)},
                {"1603128300", new OvertimeOutputElement(1603128300L, 888, 58)},
                {"1603128900", new OvertimeOutputElement(1603128900L, 917, 33)},
                {"1603129500", new OvertimeOutputElement(1603129500L, 400, 15)},
                {"1603130100", new OvertimeOutputElement(1603130100L, 1329, 77)},
                {"1603130700", new OvertimeOutputElement(1603130700L, 1057, 99)},
                {"1603131300", new OvertimeOutputElement(1603131300L, 771, 100)},
                {"1603131900", new OvertimeOutputElement(1603131900L, 1158, 119)},
                {"1603132500", new OvertimeOutputElement(1603132500L, 1658, 57)}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>())
                .OrderBy(element => element.Key)
                .ToDictionary(element => element.Key, element => element.Value);


            dictionaryExpected.Should().BeEquivalentTo(resultDic,
                options => options.IncludingFields().IncludingProperties().AllowingInfiniteRecursion()
                    .IncludingNestedObjects());

            var expectedJson =
                "[{\"TimeStamp\":1603127100,\"PermitValue\":444,\"BlockValue\":21},{\"TimeStamp\":1603127700,\"PermitValue\":636,\"BlockValue\":99},{\"TimeStamp\":1603128300,\"PermitValue\":888,\"BlockValue\":58},{\"TimeStamp\":1603128900,\"PermitValue\":917,\"BlockValue\":33},{\"TimeStamp\":1603129500,\"PermitValue\":400,\"BlockValue\":15},{\"TimeStamp\":1603130100,\"PermitValue\":1329,\"BlockValue\":77},{\"TimeStamp\":1603130700,\"PermitValue\":1057,\"BlockValue\":99},{\"TimeStamp\":1603131300,\"PermitValue\":771,\"BlockValue\":100},{\"TimeStamp\":1603131900,\"PermitValue\":1158,\"BlockValue\":119},{\"TimeStamp\":1603132500,\"PermitValue\":1658,\"BlockValue\":57}]";

            var orderedExpectedJson = OrderJsonArrayString(expectedJson, "TimeStamp").ValueOr("");

            var orderedCurrentJson = OrderJsonArrayString(
                _telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "TimeStamp"
            ).ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }
    }
}