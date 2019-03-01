using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
{
    [TestClass]
    public class IdleStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;
        private InverterMessage message;

        #endregion

        #region Methods

        [TestMethod]
        [TestCategory("Constructors")]

        public void IsNotNullIdleState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var IdleState = new IdleState(parentStateMachineMock.Object, Axis.Vertical);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            Assert.IsNotNull(IdleState);
            Assert.IsNotNull(inverterMessage);
        }

        /* TEMP
        [TestMethod]
        [TestCategory("NotifyMessage")]

        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Vertical);
            var voltageDisabledState = new VoltageDisabledState(parentStateMachineMock.Object, Axis.Vertical);
            
            if (message.IsError)
            {
                //TODO Assert.IsTrue(errorState);
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)           
                if (message.ShortPayload == this.parameterValue)
                {
                    //TODO Assert.IsTrue(voltageDisabledState);
                }
        }

        */

        #endregion
    }
}
