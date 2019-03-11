using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.Calibrate
{
    [TestClass]
    public class ErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        public void IsNotNullErrorState()
        {
            var parentStateMachineMock = new Mock<IInverterStateMachine>();
            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);

            Assert.IsNotNull(errorState);
        }

        #endregion
    }
}
