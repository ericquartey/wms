using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.Homing
{
    [TestClass]
    public class HomingStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestHomingStateMachineCreate()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var calibrateMessageData = new Mock<ICalibrateMessageData>();

            calibrateMessageData.Setup(c => c.AxisToCalibrate).Returns(Axis.Vertical);
            calibrateMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var sm = new HomingStateMachine(eventAggregatorMock.Object, calibrateMessageData.Object);

            Assert.IsNotNull(sm);
        }

        #endregion
    }
}
