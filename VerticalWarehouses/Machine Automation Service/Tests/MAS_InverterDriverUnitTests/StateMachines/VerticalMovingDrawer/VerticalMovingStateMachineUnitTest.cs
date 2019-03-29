using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.VerticalMovingDrawer
{
    [TestClass]
    public class VerticalMovingStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        public void IsNotNullVerticalMovingStateMachine()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            var verticalMovingStateMachine = new VerticalMovingStateMachine(Axis.Vertical, inverterCommandQueue, priorityInverterCommandQueue);

            Assert.IsNotNull(verticalMovingStateMachine);
        }

        [TestMethod]
        public void IsTrueChangeState()
        {
            var newStateMock = new Mock<IInverterState>();

            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var verticalMovingStateMachine = new VerticalMovingStateMachine(Axis.Vertical, inverterCommandQueue, priorityInverterCommandQueue);

            verticalMovingStateMachine.ChangeState(newStateMock.Object);

            //TODO Assert.IsTrue(verticalMovingStateMachine.ChangeState);
        }

        [TestMethod]
        public void IsTrueStart()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            var verticalMovingStateMachine = new VerticalMovingStateMachine(Axis.Vertical, inverterCommandQueue, priorityInverterCommandQueue);

            verticalMovingStateMachine.Start();

            //TODO Assert.IsTrue(verticalMovingStateMachine.Start);
        }

        #endregion
    }
}
