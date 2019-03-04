using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
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
            var endState = new EndState(parentStateMachineMock.Object, Axis.Vertical);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            Assert.IsNotNull(endState);
            Assert.IsNotNull(inverterMessage);
        }

        /* TEMP
        [TestMethod]
        public void IsTrueNotifyMessage()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();

            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Vertical);

            if (message.IsError)
            {
               Assert.IsTrue(errorState);
            }
        }
       */
        #endregion
    }
}
