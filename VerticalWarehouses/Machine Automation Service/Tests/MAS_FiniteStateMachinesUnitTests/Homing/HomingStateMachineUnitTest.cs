using System;
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
        public void TestHomingStateMachineChangeStateToEndState_Success()
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
            sm.ChangeState(new HomingEndState(sm, Axis.Horizontal), null);

            Assert.AreEqual(sm.GetState.Type, "HomingEndState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineChangeToCalibrateAxisDoneState_Success()
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
            sm.ChangeState(new HomingCalibrateAxisDoneState(sm, Axis.Horizontal), null);

            Assert.AreEqual(sm.GetState.Type, "HomingCalibrateAxisDoneState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineChangeToErrorState_Success()
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
            sm.ChangeState(new HomingErrorState(sm, Axis.Vertical), null);

            Assert.AreEqual(sm.GetState.Type, "HomingErrorState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineChangeToSwitchAxisDoneState_Success()
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
            sm.ChangeState(new HomingSwitchAxisDoneState(sm, Axis.Vertical), null);

            Assert.AreEqual(sm.GetState.Type, "HomingSwitchAxisDoneState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineGetCalibrateMessageParameter()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(new CommandEvent());

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);

            Assert.AreEqual(sm.CalibrateData.AxisToCalibrate, Axis.Vertical);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineGetStateParameter()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);
            var errorState = new HomingErrorState(sm, Axis.Horizontal);
            sm.ChangeState(errorState, null);

            Assert.AreEqual(sm.GetState, errorState);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineInvalidCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();

            Assert.ThrowsException<NullReferenceException>(() => new HomingStateMachine(eventAggregatorMock.Object, null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineStart_Success()
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
        public void TestHomingStateMachineSuccessCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);
        }

        #endregion
    }
}
