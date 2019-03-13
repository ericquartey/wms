using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.Homing
{
    [TestClass]
    public class HomingCalibrateAxisDoneStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingCalibrateAxisDoneStateInvalidCreation()
        {
            Assert.ThrowsException<NullReferenceException>(() => new HomingCalibrateAxisDoneState(null, Axis.Horizontal));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingCalibrateAxisDoneStateSuccessCreation()
        {
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            var state = new HomingCalibrateAxisDoneState(parent.Object, Axis.Vertical);

            Assert.AreEqual(state.Type, "HomingCalibrateAxisDoneState");
        }

        #endregion
    }
}
