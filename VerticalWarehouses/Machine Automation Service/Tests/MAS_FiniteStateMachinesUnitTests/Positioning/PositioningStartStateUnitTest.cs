using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.Positioning
{
    [TestClass]
    public class PositioningStartStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStartStateInvalidCreation()
        {
            Assert.ThrowsException<NullReferenceException>(() => new PositioningStartState(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStartStateSuccessCreation()
        {
            var positionMessageData = new Mock<IPositioningMessageData>();

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Absolute);
            positionMessageData.Setup(c => c.TargetPosition).Returns(10.25m);
            positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IPositioningStateMachine>().Setup(p => p.PositioningData).Returns(positionMessageData.Object);

            var state = new PositioningStartState(parent.Object);

            Assert.AreEqual(state.Type, "PositioningStartState");
        }

        #endregion
    }
}
