﻿namespace MAS_FiniteStateMachinesUnitTests.Positioning
{
    //[TestClass]
    //public class PositioningErrorStateUnitTest
    //{
    //    //[TestMethod]
    //    //[TestCategory("Unit")]
    //    //public void TestPositioningErrorStateInvalidCreation()
    //    //{
    //    //    var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, 1000.0m, 20.5m, 5.5m, 10);
    //    //    Assert.ThrowsException<NullReferenceException>(() => new PositioningErrorState(null, messageData));
    //    //}

    //    //[TestMethod]
    //    //[TestCategory("Unit")]
    //    //public void TestPositioningErrorStateSuccessCreation()
    //    //{
    //    //    var positionMessageData = new Mock<IPositioningMessageData>();

    //    //    positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
    //    //    positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Relative);
    //    //    positionMessageData.Setup(c => c.TargetPosition).Returns(1050.25m);
    //    //    positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

    //    //    //var parent = new Mock<IStateMachine>();
    //    //    //parent.As<IPositioningStateMachine>().Setup(p => p.PositioningData).Returns(positionMessageData.Object);

    //    //    //var state = new PositioningErrorState(parent.Object, positionMessageData.Object);

    //    //    //Assert.AreEqual(state.Type, string.Format("PositioningErrorState {0}", Axis.Vertical));
    //    //}
    //}
}
