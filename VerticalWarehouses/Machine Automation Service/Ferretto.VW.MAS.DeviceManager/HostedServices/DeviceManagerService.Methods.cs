using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Homing;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable;
using Ferretto.VW.MAS.DeviceManager.Positioning;
using Ferretto.VW.MAS.DeviceManager.PowerEnable;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ResetFault;
using Ferretto.VW.MAS.DeviceManager.ResetSecurity;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager
{
    internal partial class DeviceManagerService
    {
        #region Methods

        private void ProcessCheckConditionMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:Processing Command {message.Type} Source {message.Source}");

            if (message.Data is ICheckConditionMessageData data)
            {
                var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
                switch (data.ConditionToCheck)
                {
                    case ConditionToCheckType.MachineIsInEmergencyState:
                        data.Result = machineResourcesProvider.IsMachineInEmergencyState;
                        break;

                    case ConditionToCheckType.DrawerIsCompletelyOnCradle:
                        data.Result = machineResourcesProvider.IsDrawerCompletelyOnCradle;
                        break;

                    case ConditionToCheckType.DrawerIsPartiallyOnCradle:
                        data.Result = machineResourcesProvider.IsDrawerPartiallyOnCradle;
                        break;

                        //TEMP Add here other condition getters
                }

                //TEMP Send a notification message
                var msg = new NotificationMessage(
                    data,
                    $"{data.ConditionToCheck} response: {data.Result}",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.CheckCondition,
                    message.RequestingBay,
                    message.RequestingBay,
                    MessageStatus.OperationEnd);
                this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
            }
        }

        private void ProcessContinueMessage(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");
            if (this.currentStateMachines.Any(x => x.Key == receivedMessage.TargetBay))
            {
                var digitalDevicesDataProvider = serviceProvider.GetRequiredService<IDigitalDevicesDataProvider>();
                var inverterList = digitalDevicesDataProvider.GetAllInvertersByBay(receivedMessage.TargetBay);
                foreach (var inverter in inverterList)
                {
                    var inverterMessage = new FieldCommandMessage(
                        null,
                        "Continue Message Command",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.ContinueMovement,
                        (byte)inverter.Index);
                    this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);
                    this.Logger.LogDebug($"Continue Message send to inverter {inverter.Index}");
                }
            }
            else
            {
                this.Logger.LogDebug($"Continue Message ignored, no active state machine for bay {receivedMessage.TargetBay}");
            }
        }

        private void ProcessHomingMessage(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (receivedMessage.Data is IHomingMessageData data)
            {
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var targetBay = baysDataProvider.GetByAxis(data);
                if (targetBay == BayNumber.None)
                {
                    targetBay = receivedMessage.RequestingBay;
                }

                if (this.currentStateMachines.TryGetValue(targetBay, out var currentStateMachine))
                {
                    this.SendCriticalErrorMessage(
                        new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType().Name} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
                }
                else
                {
                    receivedMessage.TargetBay = targetBay;
                    currentStateMachine = new HomingStateMachine(
                        data.AxisToCalibrate,
                        data.CalibrationType,
                        data.LoadingUnitId,
                        serviceProvider.GetRequiredService<IMachineProvider>().IsOneTonMachine(),
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        serviceProvider.GetRequiredService<IMachineResourcesProvider>(),
                        this.EventAggregator,
                        this.Logger,
                        baysDataProvider,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    this.currentStateMachines.Add(targetBay, currentStateMachine);

                    this.StartStateMachine(targetBay);
                }
            }
            else
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Positioning state machine. Wrong command message payload type", 1, MessageVerbosity.Error));
            }
        }

        private void ProcessInverterFaultResetMessage(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            var digitalDevicesDataProvider = serviceProvider.GetRequiredService<IDigitalDevicesDataProvider>();

            if (this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var currentStateMachine))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType().Name} state machine. Operation already in progress on {receivedMessage.TargetBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                var inverterList = digitalDevicesDataProvider.GetAllInvertersByBay(receivedMessage.TargetBay);

                currentStateMachine = new ResetFaultStateMachine(
                    receivedMessage,
                    inverterList,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                this.currentStateMachines.Add(receivedMessage.TargetBay, currentStateMachine);

                this.StartStateMachine(receivedMessage.TargetBay);
            }
        }

        private void ProcessInverterPowerEnable(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            if (this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var currentStateMachine))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Unable to start '{nameof(InverterPowerEnableStateMachine)}' FSM. Operation {currentStateMachine?.GetType().Name} already in progress on {receivedMessage.TargetBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                var inverterList = serviceProvider
                    .GetRequiredService<IDigitalDevicesDataProvider>()
                    .GetAllInvertersByBay(receivedMessage.TargetBay);

                currentStateMachine = new InverterPowerEnableStateMachine(
                    receivedMessage,
                    inverterList,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"3:Starting FSM {currentStateMachine.GetType().Name}");
                this.currentStateMachines.Add(receivedMessage.TargetBay, currentStateMachine);

                this.StartStateMachine(receivedMessage.TargetBay);
            }
        }

        private void ProcessInverterStopMessage()
        {
            this.Logger.LogTrace("1:Method Start");

            var inverterMessage = new FieldCommandMessage(
                null,
                "Stop Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterStop,
                (byte)InverterIndex.All);
            this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);
        }

        private void ProcessPositioningMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var machineProvider = serviceProvider.GetRequiredService<IMachineProvider>();
            var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();

            System.Diagnostics.Debug.Assert(
                message.Data is IPositioningMessageData,
                "Message data should be consistent with message.Type");

            var data = message.Data as IPositioningMessageData;

            var targetBay = baysDataProvider.GetByMovementType(data);
            if (targetBay is BayNumber.None)
            {
                targetBay = message.RequestingBay;
            }

            if (this.currentStateMachines.TryGetValue(targetBay, out var currentStateMachine))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType().Name} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
            }
            else
            {
                data.IsOneKMachine = machineProvider.IsOneTonMachine();
                data.IsStartedOnBoard = machineResourcesProvider.IsDrawerCompletelyOnCradle;

                currentStateMachine = new PositioningStateMachine(
                    message.Source,
                    message.RequestingBay,
                    targetBay,
                    data,
                    machineResourcesProvider,
                    this.EventAggregator,
                    this.Logger,
                    baysDataProvider,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                this.currentStateMachines.Add(targetBay, currentStateMachine);

                this.StartStateMachine(targetBay);
            }
        }

        private void ProcessPowerEnableMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.Data is IPowerEnableMessageData data)
            {
                var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();

                //TODO verify pre conditions (is this actually an error ?)
                if (this.currentStateMachines.TryGetValue(BayNumber.BayOne, out var currentStateMachine))
                {
                    this.Logger.LogTrace($"1:Attempt to Power Off a running State Machine {currentStateMachine.GetType().Name}");
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Info",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.PowerEnable,
                        message.RequestingBay,
                        BayNumber.BayOne,
                        MessageStatus.OperationError);

                    this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

                    return;
                }

                if (!data.Enable ||
                    (!machineResourcesProvider.IsMachineInRunningState && data.Enable)
                    )
                {
                    message.TargetBay = BayNumber.BayOne;

                    currentStateMachine = new PowerEnableStateMachine(
                        message,
                        machineResourcesProvider,
                        serviceProvider.GetRequiredService<IBaysDataProvider>(),
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.currentStateMachines.Add(BayNumber.BayOne, currentStateMachine);

                    this.StartStateMachine(BayNumber.BayOne);
                }
                else
                {
                    this.Logger.LogDebug(
                        $"Machine is already in the requested state [Actual State: {machineResourcesProvider.IsMachineInRunningState}] [Requested State: {data.Enable}]");

                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Completed",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.PowerEnable,
                        message.RequestingBay,
                        BayNumber.BayOne,
                        MessageStatus.OperationEnd);

                    this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                }
            }
        }

        private void ProcessResetSecurityMessage(CommandMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(BayNumber.BayOne, out var currentStateMachine))
            {
                this.Logger.LogTrace($"1:Attempt to Power Off a running State Machine {currentStateMachine.GetType().Name}");
                var notificationMessage = new NotificationMessage(
                    null,
                    "Power Enable Critical error",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.PowerEnable,
                    message.RequestingBay,
                    BayNumber.BayOne,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

                return;
            }

            currentStateMachine = new ResetSecurityStateMachine(
                    message.RequestingBay,
                    BayNumber.BayOne,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

            this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
            this.currentStateMachines.Add(BayNumber.BayOne, currentStateMachine);

            this.StartStateMachine(BayNumber.BayOne);
        }

        private void ProcessSensorsChangedMessage(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            // Send a field message to force the Update of sensors (input lines) to InverterDriver
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);

            // Send a field message to force the Update of sensors (input lines) to IoDriver
            foreach (var ioDevice in serviceProvider.GetRequiredService<IDigitalDevicesDataProvider>().GetAllIoDevices())
            {
                var ioDataMessage = new SensorsChangedFieldMessageData
                {
                    SensorsStatus = true
                };

                var ioMessage = new FieldCommandMessage(
                    ioDataMessage,
                    "Update IO digital input",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.SensorsChanged,
                    (byte)ioDevice.Index);

                this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(ioMessage);
                this.forceRemoteIoStatusPublish[(int)ioDevice.Index] = true;
            }

            this.forceInverterIoStatusPublish = true;
        }

        private void ProcessShutterPositioningMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(message.RequestingBay, out var currentStateMachine))
            {
                this.Logger.LogTrace($"2:Deallocation FSM {currentStateMachine?.GetType().Name}");
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType().Name} state machine. Operation already in progress on ElevatorBay",
                    1, MessageVerbosity.Error));
            }
            else
            {
                if (message.Data is IShutterPositioningMessageData data)
                {
                    var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

                    message.TargetBay = message.RequestingBay;
                    currentStateMachine = new ShutterPositioningStateMachine(data,
                        message.RequestingBay,
                        message.TargetBay,
                        baysDataProvider.GetByNumber(message.RequestingBay).Shutter.Inverter.Index,
                        serviceProvider.GetRequiredService<IMachineResourcesProvider>(),
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    this.currentStateMachines.Add(message.TargetBay, currentStateMachine);

                    this.StartStateMachine(message.TargetBay);
                }
            }
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.Logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");
            if (receivedMessage.TargetBay == BayNumber.None)
            {
                if (this.currentStateMachines.Any(x => x.Key == receivedMessage.RequestingBay))
                {
                    receivedMessage.TargetBay = receivedMessage.RequestingBay;
                }
                else
                {
                    // message received from the UI: let's stop the first active FSM
                    receivedMessage.TargetBay = this.currentStateMachines.Keys.FirstOrDefault();
                }
            }

            if (this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var currentStateMachine))
            {
                if (receivedMessage.Data is IStopMessageData data)
                {
                    currentStateMachine.Stop(data.StopReason);
                }
                else
                {
                    currentStateMachine.Stop(StopRequestReason.Stop);
                }
            }
            else
            {
                var errorNotification = new NotificationMessage(
                    receivedMessage.Data,
                    $"Bay {receivedMessage.TargetBay} is already stopped",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    receivedMessage.Type,
                    receivedMessage.RequestingBay,
                    receivedMessage.TargetBay,
                    MessageStatus.OperationEnd);

                this.Logger.LogTrace($"3:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.EventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
            }
        }

        private void StartStateMachine(BayNumber bayNumber)
        {
            var stateMachine = this.currentStateMachines[bayNumber];

            try
            {
                stateMachine?.Start();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Unable to start '{stateMachine.GetType().Name}' FSM.");
                this.SendCriticalErrorMessage(ex);
                throw;
            }
        }

        #endregion
    }
}
