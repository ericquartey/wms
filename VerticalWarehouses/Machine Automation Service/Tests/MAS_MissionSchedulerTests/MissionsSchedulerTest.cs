using System;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_MissionScheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_MissionSchedulerTests
{
    [TestClass]
    public class MissionsSchedulerTest
    {
        #region Methods

        [TestMethod]
        [TestCategory( "Unit" )]
        public void TestAddMission()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var machineAutomationService_Event = new MachineAutomationService_Event();
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<MachineAutomationService_Event>() ).Returns( machineAutomationService_Event );

            var missionScheduler = new MissionsScheduler( eventAggregatorMock.Object );

            Assert.ThrowsException<ArgumentNullException>( () => missionScheduler.AddMission( null ) );
            Assert.IsTrue( missionScheduler.AddMission( new Mission() ) );
        }

        #endregion
    }
}
