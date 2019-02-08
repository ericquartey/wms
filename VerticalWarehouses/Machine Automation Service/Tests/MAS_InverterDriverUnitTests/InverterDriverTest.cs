using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_InverterDriver;
using Moq;
using Prism.Events;

namespace MAS_InverterDriverUnitTests
{
    [TestClass]
    public class InverterDriverTest
    {
        #region Method

        [TestMethod]

        public void InverterDriver()
        {
            //--->Arrange
            var eventAggregator = new Mock<IEventAggregator>().Object;

            var inverterDriver = new NewInverterDriver(eventAggregator);

            //--->Act


            

            //--->Assert
            //Assert.IsTrue(inverterDriver.GetSensorsStates());



        }

        #endregion Method
    }
}
