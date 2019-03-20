using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.Calibrate
{
    [TestClass]
    public class CalibrateStateMachineUnitTest
    {
        #region Methods

        public void IsNotNullCalibrateStateMachine()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var loggerMock = new Mock<ILogger>();

            var calibrateStateMachine = new CalibrateAxisStateMachine(Axis.Both, inverterCommandQueue, null, loggerMock.Object);

            Assert.IsNotNull(calibrateStateMachine);
        }

        [TestMethod]
        public void IsTrueChangeState()
        {
            var newStateMock = new Mock<IInverterState>();

            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var loggerMock = new Mock<ILogger>();
            var calibrateStateMachine = new CalibrateAxisStateMachine(Axis.Both, inverterCommandQueue, null, loggerMock.Object);

            calibrateStateMachine.ChangeState(newStateMock.Object);

            //TODO Assert.IsTrue(calibrateStateMachine.ChangeState);
        }

        [TestMethod]
        public void IsTrueStart()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var loggerMock = new Mock<ILogger>();

            var calibrateStateMachine = new CalibrateAxisStateMachine(Axis.Both, inverterCommandQueue, null, loggerMock.Object);

            calibrateStateMachine.Start();

            //TODO Assert.IsTrue(calibrateStateMachine.Start);
        }

        #endregion
    }
}
