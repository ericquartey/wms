﻿namespace MAS_FiniteStateMachinesUnitTests.UpDownRepetitive
{
    //[TestClass]
    //public class UpStateUnitTest
    //{
    //    //[TestMethod]
    //    //[TestCategory("Unit")]
    //    //public void TestUpStateInvalidCreation()
    //    //{
    //    //    var messageData = new UpDownRepetitiveMessageData(550.0m, 35.75m, 350);
    //    //    Assert.ThrowsException<NullReferenceException>(() => new UpState(null, messageData));
    //    //}

    //    #region Methods

    //    [TestMethod]
    //    [TestCategory("Unit")]
    //    public void TestUpStateSuccessCreation()
    //    {
    //        var upDownMessageData = new Mock<IUpDownRepetitiveMessageData>();

    //        upDownMessageData.Setup(c => c.NumberOfRequiredCycles).Returns(5500);
    //        upDownMessageData.Setup(c => c.TargetUpperBound).Returns(4500.0m);
    //        upDownMessageData.Setup(c => c.TargetLowerBound).Returns(0.0m);
    //        upDownMessageData.Setup(c => c.Verbosity).Returns(MessageVerbosity.Info);

    //        var parent = new Mock<IStateMachine>();

    //        var state = new UpState(parent.Object, upDownMessageData.Object);

    //        Assert.AreEqual(state.Type, "UpState");
    //    }

    //    #endregion
    //}
}
