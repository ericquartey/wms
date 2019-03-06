using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
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

            var calibrateStateMachine = new CalibrateStateMachine(Axis.Both, inverterCommandQueue);

            Assert.IsNotNull(calibrateStateMachine);
        }

        [TestMethod]
        public void IsTrueChangeState()
        {
            var newStateMock = new Mock<IInverterState>();

            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var calibrateStateMachine = new CalibrateStateMachine(Axis.Both, inverterCommandQueue);

            calibrateStateMachine.ChangeState(newStateMock.Object);

            //TODO Assert.IsTrue(calibrateStateMachine.ChangeState);
        }

        [TestMethod]
        public void IsTrueStart()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            var calibrateStateMachine = new CalibrateStateMachine(Axis.Both, inverterCommandQueue);

            calibrateStateMachine.Start();

            //TODO Assert.IsTrue(calibrateStateMachine.Start);
        }

        #endregion
    }
}
