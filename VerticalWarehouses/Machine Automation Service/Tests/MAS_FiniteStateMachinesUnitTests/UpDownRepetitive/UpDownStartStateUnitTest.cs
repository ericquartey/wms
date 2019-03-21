using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.UpDownRepetitive
{
    [TestClass]
    public class UpDownStartStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownStartStateInvalidCreation()
        {
            var messageData = new UpDownRepetitiveMessageData(550.0m, 35.75m, 350);
            Assert.ThrowsException<NullReferenceException>(() => new UpDownStartState(null, messageData));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownStartStateSuccessCreation()
        {
            var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();

            upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(500);
            upDownMessageData.Setup(c => c.TargetUpperBound).Returns(7500.0m);
            upDownMessageData.Setup(c => c.TargetLowerBound).Returns(0.5m);
            upDownMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();

            var state = new UpDownStartState(parent.Object, upDownMessageData.Object);

            Assert.AreEqual(state.Type, "UpDownStartState");
        }

        #endregion
    }
}
