using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.HorizontalMovingDrawer
{
    [TestClass]
    public class ErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        public void IsNotNullErrorState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Horizontal);

            Assert.IsNotNull(errorState);
        }

        #endregion
    }
}
