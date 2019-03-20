using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger>();
            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);

            Assert.IsNotNull(errorState);
        }

        #endregion
    }
}
