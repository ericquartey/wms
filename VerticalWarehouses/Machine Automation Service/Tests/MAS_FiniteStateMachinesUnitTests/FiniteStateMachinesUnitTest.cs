﻿using System;
using System.Threading;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class FiniteStateMachinesUnitTest
    {
        //[TestMethod]
        //[TestCategory("Unit")]
        //public void TestFiniteStateMachinesInvalidCreation()
        //{
        //    //Assert.ThrowsException<NullReferenceException>(() => new FiniteStateMachines(null, null));
        //}

        //[TestMethod]
        //[TestCategory("Unit")]
        //public void TestFiniteStateMachinesSuccessCreation()
        //{
        //    //var eventAggregatorMock = new Mock<IEventAggregator>();

        //    //var commandServiceEvent = new CommandEvent();
        //    //var notifyServiceEvent = new NotificationEvent();
        //    //eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(commandServiceEvent);
        //    //eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(notifyServiceEvent);

        //    //var fsm = new FiniteStateMachines(eventAggregatorMock.Object, null);

        //    //fsm.StartAsync(new CancellationToken()).Wait();
        //}
    }
}
