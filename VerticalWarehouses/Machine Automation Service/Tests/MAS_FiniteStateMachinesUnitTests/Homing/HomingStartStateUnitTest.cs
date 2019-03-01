using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.Homing
{
    [TestClass]
    public class HomingStartStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStartState_TransitionToEndState_Success()
        {
            var calibrateMessageData = new Mock<ICalibrateMessageData>();
            var parent = new Mock<IStateMachine>();
            var notifyEvent = new NotificationEvent();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);
            parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            var state = new HomingStartState(parent.Object);
            //notifyEvent.Publish(new NotificationMessage(null, "End operation", MessageActor.Any, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationEnd, ErrorLevel.NoError, MessageVerbosity.Info));

            Assert.AreEqual(state.Type, "HomingStartState");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStartStateCreate()
        {
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            var state = new HomingStartState(parent.Object);

            Assert.IsNotNull(state);
        }

        #endregion
    }
}
