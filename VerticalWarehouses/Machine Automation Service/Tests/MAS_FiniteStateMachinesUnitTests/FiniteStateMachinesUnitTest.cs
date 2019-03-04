using System.Threading;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class FiniteStateMachinesUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestFiniteStateMachinesCreate()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();

            var commandServiceEvent = new CommandEvent();
            var notifyServiceEvent = new NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(commandServiceEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(notifyServiceEvent);

            var fsm = new FiniteStateMachines(inverterDriverMock.Object, remoteIODriverMock.Object, eventAggregatorMock.Object);

            fsm.StartAsync(new CancellationToken()).Wait();

            Assert.IsNotNull(fsm);
        }

        #endregion
    }
}
