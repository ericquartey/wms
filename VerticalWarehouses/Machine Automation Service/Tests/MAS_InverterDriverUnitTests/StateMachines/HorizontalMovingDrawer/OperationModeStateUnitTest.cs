using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.HorizontalMovingDrawer
{
    [TestClass]
    public class OperationModeStateUnitTest
    {
        #region Fields

        private readonly ushort parameterValue;
        private InverterMessage message;

        #endregion

        #region Method

        [TestMethod]
        [TestCategory("Unit")]

        public void IsNotNullOperationModeState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var operationModeState = new OperationModeState(parentStateMachineMock.Object, Axis.Horizontal);
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            Assert.IsNotNull(operationModeState);
            Assert.IsNotNull(inverterMessage);
        }

        /* TEMP
       [TestMethod]
       [TestCategory("Unit")]

       public void IsTrueNotifyMessage()
       {
           var parentStateMachineMock = new Mock<IInverterStateMachine>();

           var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Horizontal);
           var readyToSwitchOnState = new ReadyToSwitchOnState(parentStateMachineMock.Object, Axis.Horizontal);

           if (message.IsError)
           {
               Assert.IsTrue(errorState);
           }

           if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
               if (message.ShortPayload == this.parameterValue)
               {
                  Assert.IsTrue(readyToSwitchOnState);
               }
       }
       */
        #endregion
    }
}
