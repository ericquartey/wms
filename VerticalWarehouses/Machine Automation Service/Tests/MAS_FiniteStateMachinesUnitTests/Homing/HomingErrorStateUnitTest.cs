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
    public class HomingErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingErrorStateInvalidCreation()
        {
            var loggerMock = new Mock<ILogger>();
            Assert.ThrowsException<NullReferenceException>(() => new HomingErrorState(null, Axis.Vertical, loggerMock.Object));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingErrorStateSuccessCreation()
        {
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            var loggerMock = new Mock<ILogger>();

            var state = new HomingErrorState(parent.Object, Axis.Horizontal, loggerMock.Object);

            Assert.AreEqual(state.Type, "HomingErrorState");
        }

        #endregion
    }
}
