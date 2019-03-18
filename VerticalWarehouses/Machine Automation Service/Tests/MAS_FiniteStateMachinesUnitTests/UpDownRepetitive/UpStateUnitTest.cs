using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MAS_FiniteStateMachinesUnitTests.UpDownRepetitive
{
    [TestClass]
    public class UpStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpStateInvalidCreation()
        {
            Assert.ThrowsException<NullReferenceException>(() => new UpState(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpStateSuccessCreation()
        {
            var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();

            upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(5500);
            upDownMessageData.Setup(c => c.TargetUpperBound).Returns(4500.0m);
            upDownMessageData.Setup(c => c.TargetLowerBound).Returns(0.0m);
            upDownMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

            var parent = new Mock<IStateMachine>();
            parent.As<IUpDownRepetitiveStateMachine>().Setup(p => p.UpDownRepetitiveData).Returns(upDownMessageData.Object);

            var state = new UpState(parent.Object);

            Assert.AreEqual(state.Type, "UpState");
        }

        #endregion
    }
}
