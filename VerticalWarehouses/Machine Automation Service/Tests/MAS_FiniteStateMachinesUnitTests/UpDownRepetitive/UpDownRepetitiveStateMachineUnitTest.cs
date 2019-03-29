using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.UpDownRepetitive
{
    [TestClass]
    public class UpDownRepetitiveStateMachineUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownRepetitiveStateMachineGetStateParameter()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());

            upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(325);
            upDownMessageData.Setup(c => c.TargetUpperBound).Returns(12000.0m);
            upDownMessageData.Setup(c => c.TargetLowerBound).Returns(125.75m);

            var sm = new UpDownRepetitiveStateMachine(eventAggregatorMock.Object, upDownMessageData.Object);
            var upState = new UpState(sm, upDownMessageData.Object);
            sm.ChangeState(upState);

            Assert.AreEqual(sm.GetState, upState);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownRepetitiveStateMachineInvalidCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();

            Assert.ThrowsException<NullReferenceException>(() => new UpDownRepetitiveStateMachine(eventAggregatorMock.Object, null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownRepetitiveStateMachineSuccessCreation()
        {
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();

            upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(1500);
            upDownMessageData.Setup(c => c.TargetUpperBound).Returns(950.5m);
            upDownMessageData.Setup(c => c.TargetLowerBound).Returns(0.25m);
            upDownMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Debug);

            var sm = new UpDownRepetitiveStateMachine(eventAggregatorMock.Object, upDownMessageData.Object);
        }

        #endregion
    }
}
