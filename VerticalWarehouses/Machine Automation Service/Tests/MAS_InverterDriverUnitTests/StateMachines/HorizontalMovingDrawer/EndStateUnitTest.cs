﻿using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.HorizontalMovingDrawer
{
    [TestClass]
    public class EndStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;

        private InverterMessage message;

        #endregion

        #region Methods

        [TestMethod]
        public void IsNotNullEndState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var endState = new EndState(parentStateMachineMock.Object, Axis.Horizontal);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

            Assert.IsNotNull(endState);
            Assert.IsNotNull(inverterMessage);
        }

        #endregion

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Horizontal);

            if (message.IsError)
            {
               Assert.IsTrue(errorState);
            }
        }
       */
    }
}
