using System;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_MachineManager;
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
        [TestCategory("Unit")]
        public void TestAddMission()
        {
            var machineManagerMock = new Mock<IMachineManager>().Object;
            var writeLogServiceMock = new Mock<IWriteLogService>().Object;
            var eventAggregatorMock = new Mock<IEventAggregator>().Object;
            var missionScheduler = new MissionsScheduler(machineManagerMock, writeLogServiceMock, eventAggregatorMock);

            Assert.ThrowsException<ArgumentNullException>(() => missionScheduler.AddMission(null));
            Assert.IsTrue(missionScheduler.AddMission(new Mission()));
        }

        #endregion
    }
}
