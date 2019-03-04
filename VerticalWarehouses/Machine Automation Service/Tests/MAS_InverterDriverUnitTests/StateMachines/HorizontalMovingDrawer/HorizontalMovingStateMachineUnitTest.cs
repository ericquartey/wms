using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_InverterDriverUnitTests.StateMachines.HorizontalMovingDrawer
{
    [TestClass]
    public class HorizontalMovingStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        public void IsNotNullHorizontalMovingStateMachine()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            var horizontalMovingStateMachine = new HorizontalMovingStateMachine(Axis.Horizontal, inverterCommandQueue, priorityInverterCommandQueue);

            Assert.IsNotNull(horizontalMovingStateMachine);


        }

        [TestMethod]
        public void IsTrueChangeState()
        {
            var newStateMock = new Mock<IInverterState>();

            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var horizontalMovingStateMachine = new HorizontalMovingStateMachine(Axis.Horizontal, inverterCommandQueue, priorityInverterCommandQueue);

            horizontalMovingStateMachine.ChangeState(newStateMock.Object);

            //TODO Assert.IsTrue(horizontalMovingStateMachine.ChangeState);
        }

        [TestMethod]
        public void IsTrueStart()
        {
            var inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();
            var priorityInverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            var horizontalMovingStateMachine = new HorizontalMovingStateMachine(Axis.Horizontal, inverterCommandQueue, priorityInverterCommandQueue);

            horizontalMovingStateMachine.Start();

            //TODO Assert.IsTrue(horizontalMovingStateMachine.Start);
        }

        #endregion
    }
}
