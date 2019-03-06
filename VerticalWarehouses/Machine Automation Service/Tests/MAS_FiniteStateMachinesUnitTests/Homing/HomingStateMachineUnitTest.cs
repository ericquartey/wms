using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.Homing
{
    [TestClass]
    public class HomingStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineCreate()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);

            Assert.IsNotNull(sm);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineStart()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            var cmdEvent = new CommandEvent();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(cmdEvent);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);
            sm.Start();

            Assert.AreEqual(sm.GetState.Type, "HomingStartState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineStartStateToEndState()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            var cmdEvent = new CommandEvent();
            var notifyEvent = new NotificationEvent();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(cmdEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(notifyEvent);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);
            sm.ChangeState(new HomingEndState(sm), null);

            Assert.AreEqual(sm.GetState.Type, "HomingEndState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineStartStateToErrorState()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            var cmdEvent = new CommandEvent();
            var notifyEvent = new NotificationEvent();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(cmdEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(notifyEvent);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);
            sm.ChangeState(new HomingErrorState(sm), null);

            Assert.AreEqual(sm.GetState.Type, "HomingErrorState");
        }

        #endregion
    }
}
