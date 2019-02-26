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
    public class DataLayerUnitTest : DBUnitTest
    {
        #region Methods

        [TestMethod]
        public void TestMethodDataLayer()
        {
            using (var context = this.CreateContext())
            {
                var updateFeedback1 = false;

                var cell = new Cell() { CellId = 1, Coord = 1, Priority = 1, Side = Side.FrontEven, Status = Status.Free };
                var listCells1 = new List<Cell>() { };
                var listCells2 = new List<Cell>() { cell };
                var updateFeedback2 = false;

                var setIntResolution = 1024;
                var setDecimalHomingCreepSpeed = 10.1m;
                var setStringHomingFastSpeed = "1000";
                int resolution;
                decimal homingCreepSpeed;
                string homingFastSpeed;

                var setIntHomingDone = 1;
                var setDecHomingDone = 1.0m;
                var setStrHomingDone = "Homing Done";
                int intHomingDone;

                var mockEventAggregator = new Mock<IEventAggregator>();
                mockEventAggregator.Setup(s => s.GetEvent<CommandEvent>()).Returns(new CommandEvent());
                var dataLayer = new DataLayer("Data Source=./TestDataBase.db", context, mockEventAggregator.Object);

                updateFeedback1 = dataLayer.LogWriting("Unit Test");

                updateFeedback2 = dataLayer.SetCellList(listCells1);

                dataLayer.SetIntegerConfigurationValue(ConfigurationValueEnum.resolution, setIntResolution);
                resolution = dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.resolution);
                dataLayer.SetDecimalConfigurationValue(ConfigurationValueEnum.homingCreepSpeed, setDecimalHomingCreepSpeed);
                homingCreepSpeed = dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.homingCreepSpeed);
                dataLayer.SetStringConfigurationValue(ConfigurationValueEnum.homingFastSpeed, setStringHomingFastSpeed);
                homingFastSpeed = dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.homingFastSpeed);

                dataLayer.SetIntegerRuntimeValue(RuntimeValueEnum.homingDone, setIntHomingDone);
                intHomingDone = dataLayer.GetIntegerRuntimeValue(RuntimeValueEnum.homingDone);

                Assert.IsTrue(updateFeedback1);

                Assert.IsTrue(updateFeedback2);
                Assert.ThrowsException<ArgumentNullException>(() => dataLayer.SetCellList(listCells2));

                Assert.AreEqual(setIntResolution, resolution);
                Assert.AreEqual(setDecimalHomingCreepSpeed, homingCreepSpeed);
                Assert.AreEqual(setStringHomingFastSpeed, homingFastSpeed);
                Assert.AreEqual(setIntHomingDone, intHomingDone);

                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.SetDecimalRuntimeValue(RuntimeValueEnum.homingDone, setDecHomingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.SetStringRuntimeValue(RuntimeValueEnum.homingDone, setStrHomingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.GetDecimalRuntimeValue(RuntimeValueEnum.homingDone));
                Assert.ThrowsException<InMemoryDataLayerException>(() => dataLayer.GetStringRuntimeValue(RuntimeValueEnum.homingDone));
            }
        }

        #endregion
    }
}
