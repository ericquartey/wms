﻿using System;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger>();
            Assert.ThrowsException<NullReferenceException>(() => new HomingCalibrateAxisDoneState(null, Axis.Horizontal, loggerMock.Object));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingCalibrateAxisDoneStateSuccessCreation()
        {
            //var calibrateMessageData = new Mock<ICalibrateMessageData>();

            //calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            //calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            //var parent = new Mock<IStateMachine>();
            //parent.As<IHomingStateMachine>().Setup(p => p.CalibrateData).Returns(calibrateMessageData.Object);

            //var loggerMock = new Mock<ILogger>();

            //var state = new HomingCalibrateAxisDoneState(parent.Object, Axis.Vertical, loggerMock.Object);

            //Assert.AreEqual(state.Type, "HomingCalibrateAxisDoneState");
        }

        #endregion
    }
}
