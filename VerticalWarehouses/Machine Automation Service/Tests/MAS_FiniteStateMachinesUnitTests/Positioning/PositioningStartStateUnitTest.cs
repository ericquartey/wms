using System;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.Positioning
{
    [TestClass]
    public class PositioningStartStateUnitTest
    {
        //[TestMethod]
        //[TestCategory("Unit")]
        //public void TestPositioningStartStateInvalidCreation()
        //{
        //    var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, 1000.0m, 20.5m, 5.5m, 10);
        //    Assert.ThrowsException<NullReferenceException>(() => new PositioningStartState(null, messageData));
        //}

        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStartStateSuccessCreation()
        {
            var positionMessageData = new Mock<IPositioningMessageData>();

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
            positionMessageData.Setup(c => c.MovementType).Returns(MovementType.Absolute);
            positionMessageData.Setup(c => c.TargetPosition).Returns(10.25m);
            positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            //var parent = new Mock<IStateMachine>();
            //parent.As<IPositioningStateMachine>().Setup(p => p.PositioningData).Returns(positionMessageData.Object);

            //var state = new PositioningStartState(parent.Object, positionMessageData.Object);

            //Assert.AreEqual(state.Type, "PositioningStartState");
        }

        #endregion
    }
}
