using System;
using System.Collections.Generic;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class WriteLogServiceUnitTest : DBTestUnitTest
    {
        #region Methods

        [TestMethod]
        // [ExpectedException(typeof(ArgumentNullException), "Argument null exception.")]
        public void TestMethodWriteLogService()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                // WriteLog
                var updateFeedback1 = false;
                // Cell managment
                var cell = new Cell() { CellId = 1, Coord = 1, Priority = 1, Side = Side.FrontEven, Status = Status.Free };
                var listCells1 = new List<Cell>() { };
                var listCells2 = new List<Cell>() { cell };
                var updateFeedback2 = false;
                // Configuration
                var setIntResolution = 1024;
                var setDecimalHomingCreepSpeed = 10.1m;
                var setStringHomingFastSpeed = "1000";
                int resolution;
                decimal homingCreepSpeed;
                string homingFastSpeed;
                // Runtime
                var setIntHomingDone = 1;
                var setDecHomingDone = 1.0m;
                var setStrHomingDone = "Homing Done";
                int intHomingDone;

                var mockEventAggregator = new Mock<IEventAggregator>();
                mockEventAggregator.Setup(s => s.GetEvent<WebAPI_CommandEvent>()).Returns(new WebAPI_CommandEvent());
                var dataLayer = new DataLayer("Data Source=./TestDataBase.db", context, mockEventAggregator.Object);

                // Act
                // WriteLog
                updateFeedback1 = dataLayer.LogWriting("Unit Test");
                // Cell managment
                updateFeedback2 = dataLayer.SetCellList(listCells1);
                // Configuration
                dataLayer.SetIntegerConfigurationValue(ConfigurationValueEnum.resolution, setIntResolution);
                resolution = dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.resolution);
                dataLayer.SetDecimalConfigurationValue(ConfigurationValueEnum.homingCreepSpeed, setDecimalHomingCreepSpeed);
                homingCreepSpeed = dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.homingCreepSpeed);
                dataLayer.SetStringConfigurationValue(ConfigurationValueEnum.homingFastSpeed, setStringHomingFastSpeed);
                homingFastSpeed = dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.homingFastSpeed);
                // Runtime
                dataLayer.SetIntegerRuntimeValue(RuntimeValueEnum.homingDone, setIntHomingDone);
                intHomingDone = dataLayer.GetIntegerRuntimeValue(RuntimeValueEnum.homingDone);

                // Assert
                // WriteLog
                Assert.IsTrue(updateFeedback1);
                // Cell managment
                Assert.IsTrue(updateFeedback2);
                Assert.ThrowsException<ArgumentNullException>(() => dataLayer.SetCellList(listCells2));
                // Configuration
                Assert.AreEqual(setIntResolution, resolution);
                Assert.AreEqual(setDecimalHomingCreepSpeed, homingCreepSpeed);
                Assert.AreEqual(setStringHomingFastSpeed, homingFastSpeed);
                Assert.AreEqual(setIntHomingDone, intHomingDone);
                // Runtime
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.SetDecimalRuntimeValue(RuntimeValueEnum.homingDone, setDecHomingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.SetStringRuntimeValue(RuntimeValueEnum.homingDone, setStrHomingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.GetDecimalRuntimeValue(RuntimeValueEnum.homingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.GetStringRuntimeValue(RuntimeValueEnum.homingDone));
            }
        }

        #endregion
    }
}
