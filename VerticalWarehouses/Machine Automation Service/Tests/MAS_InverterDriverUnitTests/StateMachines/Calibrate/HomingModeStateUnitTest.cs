﻿using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.Calibrate
{
    [TestClass]
    public class HomingModeStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;

        private InverterMessage message;

        #endregion

        #region Methods

        [TestMethod]
        public void IsNotNullHomingModeState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var loggerMock = new Mock<ILogger>();
            var homingModeState = new HomingModeState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

            Assert.IsNotNull(homingModeState);
            Assert.IsNotNull(inverterMessage);
        }

        #endregion

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);
            var shutDownState = new ShutdownState(parentStateMachineMock.Object, Axis.Both);

            if (message.IsError)
            {
                Assert.IsTrue(errorState);
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                {
                    Assert.IsTrue(shutDownState);
                }
        }
       */
    }
}
