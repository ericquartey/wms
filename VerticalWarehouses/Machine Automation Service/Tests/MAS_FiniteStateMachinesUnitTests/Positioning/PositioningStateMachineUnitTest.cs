using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.Positioning
{
    [TestClass]
    public class PositioningStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStateMachineGetPositionMessageParameter()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var positionMessageData = new Mock<IPositioningMessageData>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(new CommandEvent());

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Horizontal);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Relative);
            positionMessageData.Setup(c => c.TargetPosition).Returns(-510.75m);

            var sm = new PositioningStateMachine(eventAggregatorMock.Object, positionMessageData.Object);

            Assert.AreEqual(sm.PositioningData.AxisMovement, Axis.Horizontal);
            Assert.AreEqual(sm.PositioningData.TypeOfMovement, MovementType.Relative);
            Assert.AreEqual(sm.PositioningData.TargetPosition, -510.75m);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStateMachineGetStateParameter()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var positionMessageData = new Mock<IPositioningMessageData>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Absolute);
            positionMessageData.Setup(c => c.TargetPosition).Returns(1000.0m);

            var sm = new PositioningStateMachine(eventAggregatorMock.Object, positionMessageData.Object);
            var endState = new PositioningEndState(sm);
            sm.ChangeState(endState, null);

            Assert.AreEqual(sm.GetState, endState);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStateMachineInvalidCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();

            Assert.ThrowsException<NullReferenceException>(() => new PositioningStateMachine(eventAggregatorMock.Object, null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestPositioningStateMachineSuccessCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var positionMessageData = new Mock<IPositioningMessageData>();

            positionMessageData.Setup(c => c.AxisMovement).Returns(Axis.Vertical);
            positionMessageData.Setup(c => c.TypeOfMovement).Returns(MovementType.Absolute);
            positionMessageData.Setup(c => c.TargetPosition).Returns(1000.0m);
            positionMessageData.Setup(c => c.TargetSpeed).Returns(10.25m);
            positionMessageData.Setup(c => c.TargetAcceleration).Returns(50.0m);
            positionMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var sm = new PositioningStateMachine(eventAggregatorMock.Object, positionMessageData.Object);
        }

        #endregion
    }
}
