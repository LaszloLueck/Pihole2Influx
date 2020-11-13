using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class WorkersTest
    {

        [TestMethod]
        public void CheckElementsCountInWorkersObject()
        {
            var testee = Workers.GetJobsToDo().Count;

            testee.Should().Be(10);

        }
        
        
    }
}