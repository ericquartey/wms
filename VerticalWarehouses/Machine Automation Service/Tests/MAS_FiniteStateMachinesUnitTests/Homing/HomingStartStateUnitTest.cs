using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.Extensions.Logging;
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
        public void TestHomingStartStateInvalidCreation()
        {
            var loggerMock = new Mock<ILogger>();

            Assert.ThrowsException<NullReferenceException>(() => new HomingStartState(null, Axis.Vertical, loggerMock.Object));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStartStateSuccessCreation()
        {
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            var loggerMock = new Mock<ILogger>();

            var state = new HomingStartState(parent.Object, Axis.Vertical, loggerMock.Object);

            Assert.AreEqual(state.Type, "HomingStartState");
        }

        #endregion
    }
}
