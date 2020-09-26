using dck_pihole2influx.StatObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class StatsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public StatsConverterTest()
        {
            _telnetResultConverter = new StatsConverter();
        }
        
        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result.")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            Assert.AreEqual(true,true);
        }
        
        
    }
}