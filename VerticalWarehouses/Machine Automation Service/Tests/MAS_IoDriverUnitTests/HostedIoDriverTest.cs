using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Events;
using Moq;

namespace MAS_IoDriverUnitTests
{
    [TestClass]
    public class HostedIoDriverTest
    {
        #region Methods

        [TestMethod]
        public void CreateHostedService()
        {
            var aggregator = new EventAggregator();
            var modbusTransport = new Mock<IModbusTransport>();
            modbusTransport.Setup(mt => mt.Connect()).Returns(true);
            var dataLayer = new Mock<IDataLayer>();
            var hostedIoDriver = new HostedIoDriver(aggregator, modbusTransport.Object, dataLayer.Object);

            hostedIoDriver.StartAsync(new CancellationToken()).Wait();
        }

        #endregion
    }
}
