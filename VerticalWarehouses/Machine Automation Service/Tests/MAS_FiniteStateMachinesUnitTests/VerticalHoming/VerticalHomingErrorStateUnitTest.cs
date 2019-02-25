using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.VerticalHoming
{
    [TestClass]
    public class VerticalHomingErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingErrorState_Create()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingErrorState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(state);
        }

        #endregion
    }
}
