﻿using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
{
    [TestClass]
    public class ReadyToSwitchOnStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;

        private InverterMessage message;

        #endregion

        #region Methods

        [TestMethod]
        public void IsNotNullReadyToSwitchOnState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var readyToSwitchOnState = new ReadyToSwitchOnState(parentStateMachineMock.Object, Axis.Vertical);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

            Assert.IsNotNull(readyToSwitchOnState);
            Assert.IsNotNull(inverterMessage);
        }

        #endregion

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Vertical);
            var switchOnState = new SwitchOnState(parentStateMachineMock.Object, Axis.Vertical);

            if (message.IsError)
            {
                Assert.IsTrue(errorState);
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                {
                   Assert.IsTrue(switchOnState);
                }
        }
        */
    }
}
