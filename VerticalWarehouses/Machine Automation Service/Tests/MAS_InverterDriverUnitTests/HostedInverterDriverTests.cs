using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_InverterDriverUnitTests
{
    //[TestClass]
    //public class HostedInverterDriverTests
    //{
    //    #region Methods

    //    [TestMethod]
    //    public void CreateHostedService()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new SocketTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedInverterDriver>>();

    //        //=== Act
    //        HostedInverterDriverTestable inverterDriver = new HostedInverterDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        //=== Assert
    //        Assert.IsNotNull(inverterDriver);
    //    }

    //    [TestMethod]
    //    public void ExecuteHoming()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new SocketTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedInverterDriver>>();
    //        HostedInverterDriverTestable inverterDriver = new HostedInverterDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        inverterDriver.ExecuteAsync(token);

    //        FieldNotificationMessage inverterNotification = null;
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Subscribe(
    //            notificationMessage => { inverterNotification = new FieldNotificationMessage(notificationMessage); },
    //            ThreadOption.PublisherThread,
    //            false,
    //            notificationMessage => notificationMessage.Source == FieldMessageActor.InverterDriver);

    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(2000);

    //        //=== Act
    //        var calibrateAxisCommandData = new CalibrateAxisFieldMessageData(Axis.Horizontal);
    //        var calibrateAxisCommand = new FieldCommandMessage(calibrateAxisCommandData, "description", FieldMessageActor.InverterDriver,
    //            FieldMessageActor.FiniteStateMachines, FieldMessageType.CalibrateAxis);
    //        eventAggregator.GetEvent<FieldCommandEvent>().Publish(calibrateAxisCommand);

    //        Thread.Sleep(3000);

    //        //source.Cancel();

    //        //=== Assert
    //        Axis axisToCalibrate = (inverterNotification.Data as CalibrateAxisFieldMessageData)?.AxisToCalibrate ?? Axis.None;

    //        Assert.AreEqual(inverterNotification.Type, FieldMessageType.CalibrateAxis);
    //        Assert.AreEqual(axisToCalibrate, Axis.Horizontal);
    //        Assert.AreEqual(inverterNotification.Status, MessageStatus.OperationEnd);
    //    }

    //    [TestMethod]
    //    public void ExecuteStop()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new SocketTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedInverterDriver>>();
    //        HostedInverterDriverTestable inverterDriver = new HostedInverterDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        inverterDriver.ExecuteAsync(token);

    //        FieldNotificationMessage inverterNotification = null;
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Subscribe(
    //            notificationMessage => { inverterNotification = new FieldNotificationMessage(notificationMessage); },
    //            ThreadOption.PublisherThread,
    //            false,
    //            notificationMessage => notificationMessage.Source == FieldMessageActor.InverterDriver);

    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(2000);

    //        //=== Act
    //        var resetCommandData = new ResetInverterFieldMessageData(Axis.Horizontal);
    //        var resetCommand = new FieldCommandMessage(resetCommandData, "description", FieldMessageActor.InverterDriver,
    //            FieldMessageActor.FiniteStateMachines, FieldMessageType.InverterReset);
    //        eventAggregator.GetEvent<FieldCommandEvent>().Publish(resetCommand);

    //        Thread.Sleep(3000);

    //        //source.Cancel();

    //        //=== Assert
    //        Axis stoppedAxis = (inverterNotification.Data as IResetInverterFieldMessageData)?.AxisToStop ?? Axis.None;

    //        Assert.AreEqual(inverterNotification.Type, FieldMessageType.InverterReset);
    //        Assert.AreEqual(stoppedAxis, Axis.Horizontal);
    //        Assert.AreEqual(inverterNotification.Status, MessageStatus.OperationEnd);
    //    }

    //    [TestMethod]
    //    public void StartHostedService()
    //    {
    //        //=== Arrange
    //        var eventAggregator = new EventAggregator();
    //        var modbusTransport = new SocketTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedInverterDriver>>();
    //        HostedInverterDriverTestable inverterDriver = new HostedInverterDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        inverterDriver.ExecuteAsync(token);

    //        Thread.Sleep(100);

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
    //        var modbusTransport = new SocketTransportMock();
    //        var configurationValue = new Mock<IDataLayerConfigurationValueManagment>();
    //        var logger = new Mock<ILogger<HostedInverterDriver>>();
    //        HostedInverterDriverTestable inverterDriver = new HostedInverterDriverTestable(eventAggregator, modbusTransport, configurationValue.Object, logger.Object);

    //        CancellationTokenSource source = new CancellationTokenSource();
    //        CancellationToken token = source.Token;
    //        inverterDriver.ExecuteAsync(token);

    //        Thread.Sleep(100);

    //        //=== Act
    //        var dataReadyNotification = new FieldNotificationMessage(null, "description", FieldMessageActor.Any,
    //            FieldMessageActor.DataLayer, FieldMessageType.DataLayerReady, MessageStatus.NoStatus);
    //        eventAggregator.GetEvent<FieldNotificationEvent>().Publish(dataReadyNotification);

    //        Thread.Sleep(100);

    //        source.Cancel();

    //        //=== Assert
    //        Assert.IsTrue(token.IsCancellationRequested);
    //    }

    //    #endregion

    //    #region Classes

    //    private class HostedInverterDriverTestable : HostedInverterDriver
    //    {
    //        #region Constructors

    //        public HostedInverterDriverTestable(IEventAggregator eventAggregator, ISocketTransport socketTransport, IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement, ILogger<HostedInverterDriver> logger) :
    //            base(eventAggregator, socketTransport, dataLayerConfigurationValueManagement, logger)
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
