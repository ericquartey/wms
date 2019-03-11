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
    public class PositioningErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningErrorStateInvalidCreation()
        {
            Assert.ThrowsException<NullReferenceException>(() => new PositioningErrorState(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningErrorStateSuccessCreation()
        {
            var positionMessageData = new Mock<IPositioningMessageData>();

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Relative);
            positionMessageData.Setup(c => c.TargetPosition).Returns(1050.25m);
            positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IPositioningStateMachine>().Setup(p => p.PositioningData).Returns(positionMessageData.Object);

            var state = new PositioningErrorState(parent.Object);

            Assert.AreEqual(state.Type, string.Format("PositioningErrorState {0}", Axis.Vertical));
        }

        #endregion
    }
}
