using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopClientsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopClientsConverterTest()
        {
            _telnetResultConverter = new TopClientsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"0 24593 192.168.1.1 aaa.localdomain
1 8136 192.168.1.227 bbb.localdomain
2 4704 192.168.1.225 ccc.localdomain
3 3741 192.168.1.174 ddd.localdomain
4 2836 192.168.1.231 eee.localdomain
5 2587 192.168.1.120 
6 2035 192.168.1.196 fff.localdomain
7 2009 192.168.1.226 ggg.localdomain
8 1952 192.168.1.167 hhh.localdomain
9 1807 192.168.1.137 


---EOM---

";
            _telnetResultConverter.Convert(testee).Wait();

        }
        
        
    }
}