using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
{
    [TestClass]
    public class ErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        public void IsNotNullErrorState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Vertical);

            Assert.IsNotNull(errorState);
        }

        #endregion
    }
}
