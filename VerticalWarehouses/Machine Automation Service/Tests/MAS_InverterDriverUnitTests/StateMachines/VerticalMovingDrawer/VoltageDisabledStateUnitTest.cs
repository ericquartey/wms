using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
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
            var voltageDisabledState = new VoltageDisabledState(parentStateMachineMock.Object, Axis.Vertical);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            Assert.IsNotNull(voltageDisabledState);
            Assert.IsNotNull(inverterMessage);
        }

        #endregion

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Vertical);
            var operationModeState = new OperationModeState(parentStateMachineMock.Object, Axis.Vertical);

            if (message.IsError)
            {
                Assert.IsTrue(errorState);
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                {
                    Assert.IsTrue(operationModeState);
                }
        }
       */
    }
}
