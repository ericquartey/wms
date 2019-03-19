using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.Positioning
{
    [TestClass]
    public class PositioningEndStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositionigEndStateInvalidCreation()
        {
            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, 1000.0m, 20.5m, 5.5m, 10);
            Assert.ThrowsException<NullReferenceException>(() => new PositioningEndState(null, messageData));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningEndStateSuccessCreation()
        {
            var positionMessageData = new Mock<IPositioningMessageData>();

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Horizontal);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Absolute);
            positionMessageData.Setup(c => c.TargetPosition).Returns(-125.0m);
            positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IPositioningStateMachine>().Setup(p => p.PositioningData).Returns(positionMessageData.Object);

            var state = new PositioningEndState(parent.Object, positionMessageData.Object);

            Assert.AreEqual(state.Type, string.Format("PositioningEndState {0}", Axis.Horizontal));
        }

        #endregion
    }
}
