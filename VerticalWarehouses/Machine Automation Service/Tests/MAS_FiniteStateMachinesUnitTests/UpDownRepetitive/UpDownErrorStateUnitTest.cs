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
    public class UpDownErrorStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownErrorStateInvalidCreation()
        {
            Assert.ThrowsException<NullReferenceException>(() => new UpDownErrorState(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestUpDownErrorStateSuccessCreation()
        {
            var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();

            upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(1350);
            upDownMessageData.Setup(c => c.TargetUpperBound).Returns(550.5m);
            upDownMessageData.Setup(c => c.TargetLowerBound).Returns(15.75m);
            upDownMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Debug);

            var parent = new Mock<IStateMachine>();
            parent.As<IUpDownRepetitiveStateMachine>().Setup(p => p.UpDownRepetitiveData).Returns(upDownMessageData.Object);

            var state = new UpDownErrorState(parent.Object);

            Assert.AreEqual(state.Type, "UpDownErrorState");
        }

        #endregion
    }
}
