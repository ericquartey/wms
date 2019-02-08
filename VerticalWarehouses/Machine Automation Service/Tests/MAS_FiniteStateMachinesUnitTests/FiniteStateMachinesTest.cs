using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class FiniteStateMachinesTest
    {
        #region Methods

        [TestMethod]
        public void FiniteStateMachinesCreate()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<WebAPI_CommandEvent>()).Returns(new WebAPI_CommandEvent());

            var fsm = new FiniteStateMachines(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(fsm);
        }

        #endregion
    }
}
