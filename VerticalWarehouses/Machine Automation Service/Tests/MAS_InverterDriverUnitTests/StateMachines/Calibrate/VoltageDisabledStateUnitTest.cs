﻿using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.Calibrate
{
    [TestClass]
    public class VoltageDisabledStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;

        private InverterMessage message;

        #endregion

        #region Methods

        [TestMethod]
        public void IsNotNullVoltageDisabledState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var loggerMock = new Mock<ILogger>();
            var voltageDisabledState = new VoltageDisabledState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

            Assert.IsNotNull(voltageDisabledState);
            Assert.IsNotNull(inverterMessage);
        }

        #endregion

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);
            var homingModeState = new HomingModeState(parentStateMachineMock.Object, Axis.Both);

            if (message.IsError)
            {
                Assert.IsTrue(errorState);
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                {
                    Assert.IsTrue(homingModeState);
                }
        }
       */
    }
}
