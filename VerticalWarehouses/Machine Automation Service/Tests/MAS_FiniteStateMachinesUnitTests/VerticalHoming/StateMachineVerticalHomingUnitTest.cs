using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class StateMachineVerticalHomingUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory( "Unit" )]
        public void TestStateMachineVerticalHoming_Create()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();

            var smVerticalHoming = new StateMachineVerticalHoming( inverterDriverMock.Object, eventAggregatorMock.Object );

            Assert.IsNotNull( smVerticalHoming );
        }

        [TestMethod]
        [TestCategory( "Unit" )]
        public void TestStateMachineVerticalHoming_StartSuccess()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new NotificationEvent();
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<NotificationEvent>() ).Returns( notifyDriverEvent );

            var smVerticalHoming = new StateMachineVerticalHoming( inverterDriverMock.Object, eventAggregatorMock.Object );
            smVerticalHoming.Start();

            Assert.AreEqual( smVerticalHoming.Type, "Vertical Homing Idle State" );
        }

        #endregion
    }
}
