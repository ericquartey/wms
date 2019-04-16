using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_IODriver;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_IoDriverUnitTests
{
    //[TestClass]
    //public class HostedIoDriverTest
    //{
    //    #region Methods

    //    [TestMethod]
    //    public void CompletePowerUp()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();
    //        HostedIoDriverTestable ioDriver = new HostedIoDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);
    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        ioDriver.ExecuteAsync(token);
    //        FieldNotificationMessage ioNotification = null;
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Subscribe(
    //            notificationMessage => { ioNotification = new FieldNotificationMessage(notificationMessage); },
    //            ThreadOption.PublisherThread,
    //            false,
    //            notificationMessage => notificationMessage.Source == FieldMessageActor.IoDriver);

    //        //=== Act
    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(2000);

    //        source.Cancel();

    //        //=== Assert
    //        Assert.AreEqual(ioNotification.Type, FieldMessageType.IoPowerUp);
    //        Assert.AreEqual(ioNotification.Status, MessageStatus.OperationEnd);
    //    }

    //    [TestMethod]
    //    public void CreateHostedService()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();

    //        //=== Act
    //        HostedIoDriver ioDriver = new HostedIoDriver(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        //=== Assert
    //        Assert.IsNotNull(ioDriver);
    //    }

    //    [TestMethod]
    //    public void ExecuteReset()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();
    //        HostedIoDriverTestable ioDriver = new HostedIoDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);
    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        ioDriver.ExecuteAsync(token);
    //        FieldNotificationMessage ioNotification = null;
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Subscribe(
    //            notificationMessage => { ioNotification = new FieldNotificationMessage(notificationMessage); },
    //            ThreadOption.PublisherThread,
    //            false,
    //            notificationMessage => notificationMessage.Source == FieldMessageActor.IoDriver);

    //        //=== Act
    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(2000);

    //        var resetCommand = new FieldCommandMessage(null, "description", FieldMessageActor.IoDriver,
    //            FieldMessageActor.FiniteStateMachines, FieldMessageType.IoReset);
    //        eventAggregator.GetEvent<FieldCommandEvent>().Publish(resetCommand);

    //        Thread.Sleep(2000);

    //        //source.Cancel();

    //        //=== Assert
    //        Assert.AreEqual(ioNotification.Type, FieldMessageType.IoReset);
    //        Assert.AreEqual(ioNotification.Status, MessageStatus.OperationEnd);
    //    }

    //    [TestMethod]
    //    public void ExecuteSwitch()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();
    //        HostedIoDriverTestable ioDriver = new HostedIoDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);
    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        ioDriver.ExecuteAsync(token);
    //        FieldNotificationMessage ioNotification = null;
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Subscribe(
    //            notificationMessage => { ioNotification = new FieldNotificationMessage(notificationMessage); },
    //            ThreadOption.PublisherThread,
    //            false,
    //            notificationMessage => notificationMessage.Source == FieldMessageActor.IoDriver);

    //        //=== Act
    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(2000);

    //        var switchCOmmandData = new SwitchAxisFieldMessageData(Axis.Horizontal);
    //        var resetCommand = new FieldCommandMessage(switchCOmmandData, "description", FieldMessageActor.IoDriver,
    //            FieldMessageActor.FiniteStateMachines, FieldMessageType.SwitchAxis);
    //        eventAggregator.GetEvent<FieldCommandEvent>().Publish(resetCommand);

    //        Thread.Sleep(2000);

    //        source.Cancel();

    //        //=== Assert
    //        Assert.AreEqual(ioNotification.Type, FieldMessageType.SwitchAxis);
    //        Assert.AreEqual(ioNotification.Status, MessageStatus.OperationEnd);
    //    }

    //    [TestMethod]
    //    public void StartHostedService()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();
    //        HostedIoDriverTestable ioDriver = new HostedIoDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        ioDriver.ExecuteAsync(token);

    //        Thread.Sleep(400);

    //        //=== Act
    //        source.Cancel();

    //        //=== Assert
    //        Assert.IsTrue(token.IsCancellationRequested);
    //    }

    //    [TestMethod]
    //    public void StartHostedServiceHardware()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new ModbusTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedIoDriver>>();
    //        HostedIoDriverTestable ioDriver = new HostedIoDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);
    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        ioDriver.ExecuteAsync(token);

    //        //=== Act
    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(400);

    //        source.Cancel();

    //        //=== Assert
    //        Assert.IsTrue(token.IsCancellationRequested);
    //    }

    //    #endregion

    //    #region Classes

    //    private class HostedIoDriverTestable : HostedIoDriver
    //    {
    //        #region Constructors

    //        public HostedIoDriverTestable(IEventAggregator eventAggregator, IModbusTransport modbusTransport, IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement, ILogger<HostedIoDriver> logger) :
    //            base(eventAggregator, modbusTransport, dataLayerConfigurationValueManagement, logger)
    //        { }

    //        #endregion

    //        #region Methods

    //        public new async Task ExecuteAsync(CancellationToken stoppingToken)
    //        {
    //            await base.ExecuteAsync(stoppingToken);
    //        }

    //        #endregion
    //    }

    //    #endregion
    //}
}
