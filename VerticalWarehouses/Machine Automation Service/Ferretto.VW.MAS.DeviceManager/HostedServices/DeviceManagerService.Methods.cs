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

namespace Ferretto.VW.MAS.DeviceManager
{
    internal partial class DeviceManagerService
    {
        #region Methods

        private void ProcessBayLight(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.Data is IBayLightMessageData data)
            {
                var bayDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var ioIndex = bayDataProvider.GetIoDevice(message.TargetBay);

                // Send a field message to ligh the bay (via output line) to IoDevice
                var bayLightDataMessage = new BayLightFieldMessageData(data.Enable);
                var inverterMessage = new FieldCommandMessage(
                    bayLightDataMessage,
                    $"Bay Light={data.Enable}",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.BayLight,
                    (byte)ioIndex);
                this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);
            }
        }

        private void ProcessCheckConditionMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:Processing Command {message.Type} Source {message.Source}");

            if (message.Data is ICheckConditionMessageData data)
            {
                switch (data.ConditionToCheck)
                {
                    case ConditionToCheckType.MachineIsInEmergencyState:
                        data.Result = this.machineResourcesProvider.IsMachineInEmergencyState;
                        break;

                    case ConditionToCheckType.DrawerIsCompletelyOnCradle:
                        data.Result = this.machineResourcesProvider.IsDrawerCompletelyOnCradle;
                        break;

                    case ConditionToCheckType.DrawerIsPartiallyOnCradle:
                        data.Result = this.machineResourcesProvider.IsDrawerPartiallyOnCradle;
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
            if (this.bayInverters.Any(b => b.Value == receivedMessage.TargetBay && this.currentStateMachines.ContainsKey(b.Key)))
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

                if (this.bayInverters.Any(b => b.Value == targetBay && this.currentStateMachines.ContainsKey(b.Key)))
                {
                    this.SendCriticalErrorMessage(
                        new FsmExceptionMessageData(null, $"Error while starting Homing. Operation already in progress on {targetBay}", 1, MessageVerbosity.Error));
                }
                else
                {
                    receivedMessage.TargetBay = targetBay;
                    var currentStateMachine = new HomingStateMachine(
                        data.AxisToCalibrate,
                        data.CalibrationType,
                        data.LoadingUnitId,
                        this.machineVolatileDataProvider.IsOneTonMachine.Value,
                        data.ShowErrors,
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        this.machineResourcesProvider,
                        this.EventAggregator,
                        this.Logger,
                        baysDataProvider,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    var inverterIndex = this.bayInverters.First(b => b.Value == targetBay).Key;
                    this.currentStateMachines.Add(inverterIndex, currentStateMachine);

                    this.StartStateMachine(inverterIndex);
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

            if (this.bayInverters.Any(b => b.Value == receivedMessage.TargetBay && this.currentStateMachines.ContainsKey(b.Key)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Error while starting FaultReset state machine. Operation already in progress on {receivedMessage.TargetBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                var inverterList = digitalDevicesDataProvider.GetAllInvertersByBay(receivedMessage.TargetBay);

                var currentStateMachine = new ResetFaultStateMachine(
                    receivedMessage,
                    inverterList,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                var inverterIndex = this.bayInverters.First(b => b.Value == receivedMessage.TargetBay).Key;
                this.currentStateMachines.Add(inverterIndex, currentStateMachine);

                this.StartStateMachine(inverterIndex);
            }
        }

        private void ProcessInverterPowerEnable(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            if (this.bayInverters.Any(b => b.Value == receivedMessage.TargetBay && this.currentStateMachines.ContainsKey(b.Key)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Unable to start '{nameof(InverterPowerEnableStateMachine)}' FSM. Operation already in progress on {receivedMessage.TargetBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                var inverterList = serviceProvider
                    .GetRequiredService<IDigitalDevicesDataProvider>()
                    .GetAllInvertersByBay(receivedMessage.TargetBay);

                var currentStateMachine = new InverterPowerEnableStateMachine(
                    receivedMessage,
                    inverterList,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"3:Starting FSM {currentStateMachine.GetType().Name}");
                var inverterIndex = this.bayInverters.First(b => b.Value == receivedMessage.TargetBay).Key;
                this.currentStateMachines.Add(inverterIndex, currentStateMachine);

                this.StartStateMachine(inverterIndex);
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

            System.Diagnostics.Debug.Assert(
                message.Data is IPositioningMessageData,
                "Message data should be consistent with message.Type");

            var data = message.Data as IPositioningMessageData;

            var targetBay = baysDataProvider.GetByMovementType(data);
            if (targetBay is BayNumber.None)
            {
                targetBay = message.RequestingBay;
            }
            var shutterIndex = baysDataProvider.GetByNumber(message.RequestingBay).Shutter.Inverter.Index;

            if (this.bayInverters.Any(b => b.Value == targetBay && b.Key != shutterIndex && this.currentStateMachines.ContainsKey(b.Key)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Positioning state machine. Operation already in progress on {targetBay}", 1, MessageVerbosity.Error));
            }
            else
            {
                data.IsOneTonMachine = this.machineVolatileDataProvider.IsOneTonMachine.Value;
                data.IsStartedOnBoard = this.machineResourcesProvider.IsDrawerCompletelyOnCradle;

                var currentStateMachine = new PositioningStateMachine(
                    message.Source,
                    message.RequestingBay,
                    targetBay,
                    data,
                    this.machineResourcesProvider,
                    this.EventAggregator,
                    this.Logger,
                    baysDataProvider,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                var inverterIndex = this.bayInverters.First(b => b.Value == targetBay && b.Key != shutterIndex).Key;
                this.currentStateMachines.Add(inverterIndex, currentStateMachine);

                this.StartStateMachine(inverterIndex);
            }
        }

        private void ProcessPowerEnableMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.Data is IPowerEnableMessageData data)
            {
                //TODO verify pre conditions (is this actually an error ?)
                var inverter = this.bayInverters.First(b => b.Value == BayNumber.BayOne).Key;
                if (this.currentStateMachines.TryGetValue(inverter, out var currentStateMachine))
                {
                    this.Logger.LogTrace($"1:Attempt to Power On a running State Machine {currentStateMachine.GetType().Name}");
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
                    (!this.machineResourcesProvider.IsMachineInRunningState && data.Enable)
                    )
                {
                    message.TargetBay = BayNumber.BayOne;

                    currentStateMachine = new PowerEnableStateMachine(
                        message,
                        this.machineResourcesProvider,
                        serviceProvider.GetRequiredService<IBaysDataProvider>(),
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.currentStateMachines.Add(inverter, currentStateMachine);

                    this.StartStateMachine(inverter);

                    if (!data.Enable)
                    {
                        this.machineVolatileDataProvider.ElevatorVerticalPositionOld = this.machineVolatileDataProvider.ElevatorVerticalPosition;

                        if (this.machineVolatileDataProvider.IsBayLightOn.Count > 0)
                        {
                            for (var bayNumber = BayNumber.BayOne; bayNumber < BayNumber.ElevatorBay; bayNumber++)
                            {
                                if (this.machineVolatileDataProvider.IsBayLightOn.ContainsKey(bayNumber)
                                    && this.machineVolatileDataProvider.IsBayLightOn[bayNumber]
                                    )
                                {
                                    this.machineVolatileDataProvider.IsBayLightOn[bayNumber] = false;

                                    this.EventAggregator
                                        .GetEvent<NotificationEvent>()
                                        .Publish(
                                            new NotificationMessage(
                                                null,
                                                $"BayLight={false} completed, Bay={bayNumber}",
                                                MessageActor.Any,
                                                MessageActor.DeviceManager,
                                                MessageType.BayLight,
                                                bayNumber,
                                                bayNumber,
                                                MessageStatus.OperationEnd));
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.Logger.LogDebug(
                        $"Machine is already in the requested state [Actual State: {this.machineResourcesProvider.IsMachineInRunningState}] [Requested State: {data.Enable}]");

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

            var inverter = this.bayInverters.First(b => b.Value == BayNumber.BayOne).Key;
            if (this.currentStateMachines.TryGetValue(inverter, out var currentStateMachine))
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
            this.currentStateMachines.Add(inverter, currentStateMachine);

            this.StartStateMachine(inverter);
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
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var shutterIndex = baysDataProvider.GetByNumber(message.RequestingBay).Shutter.Inverter.Index;

            if (this.bayInverters.Any(b => b.Value == message.RequestingBay && b.Key == shutterIndex && this.currentStateMachines.ContainsKey(b.Key)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null,
                    $"Error while starting shutter position state machine. Operation already in progress on {message.RequestingBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                if (message.Data is IShutterPositioningMessageData data)
                {
                    message.TargetBay = message.RequestingBay;
                    var currentStateMachine = new ShutterPositioningStateMachine(data,
                        message.RequestingBay,
                        message.TargetBay,
                        shutterIndex,
                        this.machineResourcesProvider,
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    this.currentStateMachines.Add(shutterIndex, currentStateMachine);

                    this.StartStateMachine(shutterIndex);
                }
            }
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.Logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");
            if (receivedMessage.TargetBay == BayNumber.None)
            {
                if (this.bayInverters.Any(b => b.Value == receivedMessage.RequestingBay && this.currentStateMachines.ContainsKey(b.Key)))
                {
                    receivedMessage.TargetBay = receivedMessage.RequestingBay;
                }
                else
                {
                    // message received from the UI: let's stop the first active FSM
                    if (this.currentStateMachines.Any())
                    {
                        receivedMessage.TargetBay = this.bayInverters.First(x => x.Key == this.currentStateMachines.Keys.First()).Value;
                    }
                }
            }

            var stopped = false;
            foreach (var inverter in this.bayInverters.Where(x => x.Value == receivedMessage.TargetBay).Select(s => s.Key))
            {
                if (this.currentStateMachines.TryGetValue(inverter, out var currentStateMachine))
                {
                    if (receivedMessage.Data is IStopMessageData data)
                    {
                        currentStateMachine.Stop(data.StopReason);
                    }
                    else
                    {
                        currentStateMachine.Stop(StopRequestReason.Stop);
                    }
                    stopped = true;
                }
            }
            if (!stopped)
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

        private void ProcessStopTest(CommandMessage receivedMessage)
        {
            this.Logger.LogTrace("1:Method Start");
            bool stopped = false;
            foreach (var inverter in this.bayInverters.Where(x => x.Value == receivedMessage.RequestingBay).Select(s => s.Key))
            {
                this.currentStateMachines.TryGetValue(inverter, out var fsm);
                if (fsm is PositioningStateMachine)
                {
                    fsm.ProcessCommandMessage(receivedMessage);
                    stopped = true;
                }
            }
            if (!stopped)
            {
                this.Logger.LogDebug($"StopTest Message ignored, no active positioning state machine for bay {receivedMessage.TargetBay}");
            }
        }

        private void StartStateMachine(InverterIndex inverterIndex)
        {
            var stateMachine = this.currentStateMachines[inverterIndex];

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
