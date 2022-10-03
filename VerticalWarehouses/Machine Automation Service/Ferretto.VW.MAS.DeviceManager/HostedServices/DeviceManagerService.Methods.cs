using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements;
using Ferretto.VW.MAS.DeviceManager.ExtBayPositioning;
using Ferretto.VW.MAS.DeviceManager.Homing;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution;
using Ferretto.VW.MAS.DeviceManager.InverterPogramming;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable;
using Ferretto.VW.MAS.DeviceManager.InverterReading;
using Ferretto.VW.MAS.DeviceManager.Positioning;
using Ferretto.VW.MAS.DeviceManager.PowerEnable;
using Ferretto.VW.MAS.DeviceManager.ProfileResolution;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements;
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

        private void ProcessCheckIntrusion(CommandMessage receivedMessage)
        {
            this.Logger.LogTrace("1:Method Start");

            if (receivedMessage.Data is CheckIntrusionMessageData data)
            {
                var isActive = this.currentStateMachines.Any(c => c.BayNumber == receivedMessage.TargetBay && c is CheckIntrusionStateMachine);
                if (data.Enable
                    && !isActive
                    )
                {
                    var currentStateMachine = new CheckIntrusionStateMachine(
                        receivedMessage,
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
                else if (!data.Enable
                    && isActive
                    )
                {
                    var currentStateMachine = this.currentStateMachines.FirstOrDefault(c => c.BayNumber == receivedMessage.TargetBay && c is CheckIntrusionStateMachine);

                    this.Logger.LogTrace($"2:Stop FSM {currentStateMachine?.GetType().Name}");
                    currentStateMachine?.Stop(StopRequestReason.NoReason);
                }
                else
                {
                    var msg = new NotificationMessage(
                        null,
                        $"Check intrusion completed for bay {receivedMessage.TargetBay}",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.CheckIntrusion,
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        MessageStatus.OperationEnd);

                    this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
            }
        }

        private void ProcessCombinedMovemets(CommandMessage message, IServiceProvider serviceProvider)
        {
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            System.Diagnostics.Debug.Assert(
                message.Data is ICombinedMovementsMessageData,
                "Message data should be consistent with message.Type");

            var data = message.Data as ICombinedMovementsMessageData;
            var targetBay = BayNumber.ElevatorBay;

            if (this.currentStateMachines.Any(x => x.BayNumber == targetBay && !(x is ShutterPositioningStateMachine)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Combined Movements state machine. Operation already in progress on {targetBay}", 1, MessageVerbosity.Error));
            }
            else
            {
                var currentStateMachine = new CombinedMovementsStateMachine(
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
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
            }
        }

        private void ProcessContinueMessage(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");
            if (this.currentStateMachines.Any(x => x.BayNumber == receivedMessage.TargetBay))
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

                if (this.currentStateMachines.Any(x => x.BayNumber == targetBay && !(x is ShutterPositioningStateMachine)))
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
                        data.TurnBack,
                        data.BypassSensor,
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        this.machineResourcesProvider,
                        this.EventAggregator,
                        this.Logger,
                        baysDataProvider,
                        this.ServiceScopeFactory);

                    this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
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

            if (this.currentStateMachines.Any(x => x.BayNumber == receivedMessage.TargetBay))
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
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
            }
        }

        private void ProcessInverterPowerEnable(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            if (this.currentStateMachines.Any(x => x.BayNumber == receivedMessage.TargetBay))
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
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
            }
        }

        private void ProcessInvertersProgramming(CommandMessage message, IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.Assert(
                message.Data is IInverterProgrammingMessageData,
                "Message data should be consistent with message.Type");

            if (this.currentStateMachines.Any(x => (x is InverterProgrammingStateMachine)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Inverters programming state machine. Operation already in progress", 1, MessageVerbosity.Error));
            }
            else
            {
                message.TargetBay = BayNumber.ElevatorBay;
                var data = message.Data as IInverterProgrammingMessageData;
                var messageInverterIndex = (InverterIndex)data.InverterParametersData.First().InverterIndex;
                if (messageInverterIndex != InverterIndex.None)
                {
                    var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                    message.TargetBay = baysDataProvider.GetByInverterIndex(messageInverterIndex);
                }
                var currentStateMachine = new InverterProgrammingStateMachine(
                    message,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
            }
        }

        private void ProcessInvertersReading(CommandMessage message, IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.Assert(
                message.Data is IInverterReadingMessageData,
                "Message data should be consistent with message.Type");

            if (this.currentStateMachines.Any(x => (x is InverterReadingStateMachine)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Inverters reading state machine. Operation already in progress", 1, MessageVerbosity.Error));
            }
            else
            {
                message.TargetBay = BayNumber.ElevatorBay;
                var data = message.Data as IInverterReadingMessageData;
                var messageInverterIndex = (InverterIndex)data.InverterParametersData.First().InverterIndex;
                if (messageInverterIndex != InverterIndex.None)
                {
                    var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                    message.TargetBay = baysDataProvider.GetByInverterIndex(messageInverterIndex);
                }
                var currentStateMachine = new InverterReadingStateMachine(
                    message,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

                this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
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

            if (this.currentStateMachines.Any(
                    x => x.BayNumber == targetBay &&
                    !(x is PositioningStateMachine) &&
                    !(x is ShutterPositioningStateMachine) &&
                    !(x is RepetitiveHorizontalMovementsStateMachine) &&
                    !(x is CombinedMovementsStateMachine) &&
                    !(x is ProfileResolutionStateMachine)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Positioning state machine. Operation already in progress on {targetBay}", 1, MessageVerbosity.Error));
            }
            else
            {
                data.IsOneTonMachine = this.machineVolatileDataProvider.IsOneTonMachine.Value;
                data.IsStartedOnBoard = this.machineResourcesProvider.IsDrawerCompletelyOnCradle;

                if (data.MovementMode == MovementMode.ExtBayChain ||
                    data.MovementMode == MovementMode.ExtBayChainManual ||
                    data.MovementMode == MovementMode.ExtBayTest)
                {
                    // external bay
                    var currentStateMachine = new ExtBayPositioningStateMachine(
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
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
                else if (data.MovementMode == MovementMode.HorizontalResolution)
                {
                    var currentStateMachine = new HorizontalResolutionStateMachine(
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
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
                else if (data.MovementMode == MovementMode.ProfileResolution)
                {
                    var currentStateMachine = new ProfileResolutionStateMachine(
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
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
                else
                {
                    // vertical and horizontal axes,
                    // carousel
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
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
            }
        }

        private void ProcessPowerEnableMessage(CommandMessage message, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.Data is IPowerEnableMessageData data)
            {
                //TODO verify pre conditions (is this actually an error ?)
                if (this.currentStateMachines.Any(x => x.BayNumber == BayNumber.BayOne))
                {
                    this.Logger.LogTrace($"1:Attempt to Power On a running State Machine ");
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

                    var currentStateMachine = new PowerEnableStateMachine(
                        message,
                        this.machineResourcesProvider,
                        serviceProvider.GetRequiredService<IBaysDataProvider>(),
                        this.EventAggregator,
                        this.Logger,
                        this.ServiceScopeFactory);

                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);

                    if (data.Enable)
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

        private void ProcessRepetitiveHorizontalMovements(CommandMessage message, IServiceProvider serviceProvider)
        {
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            System.Diagnostics.Debug.Assert(
                message.Data is IRepetitiveHorizontalMovementsMessageData,
                "Message data should be consistent with message.Type");

            var data = message.Data as IRepetitiveHorizontalMovementsMessageData;
            var targetBay = BayNumber.ElevatorBay;

            if (this.currentStateMachines.Any(x => x.BayNumber == targetBay && !(x is ShutterPositioningStateMachine)))
            {
                this.SendCriticalErrorMessage(new FsmExceptionMessageData(null, $"Error while starting Repetitive Horizontal Movements state machine. Operation already in progress on {targetBay}", 1, MessageVerbosity.Error));
            }
            else
            {
                var currentStateMachine = new RepetitiveHorizontalMovementsStateMachine(
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
                this.currentStateMachines.Add(currentStateMachine);

                this.StartStateMachine(currentStateMachine);
            }
        }

        private void ProcessResetSecurityMessage(CommandMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.Any(x => x.BayNumber == BayNumber.BayOne))
            {
                this.Logger.LogTrace($"1:Attempt to Power Off a running State Machine ");
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

            var currentStateMachine = new ResetSecurityStateMachine(
                    message.RequestingBay,
                    BayNumber.BayOne,
                    this.EventAggregator,
                    this.Logger,
                    this.ServiceScopeFactory);

            this.Logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType().Name}");
            this.currentStateMachines.Add(currentStateMachine);

            this.StartStateMachine(currentStateMachine);
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
            var shutterIndex = baysDataProvider.GetShutterInverterIndex(message.RequestingBay);

            if (this.currentStateMachines.Any(x => x.BayNumber == message.RequestingBay
                    && !(x is PositioningStateMachine)
                    && !(x is ExtBayPositioningStateMachine)
                    && !(x is HorizontalResolutionStateMachine)
                    && !(x is ProfileResolutionStateMachine)
                    && !(x is HomingStateMachine)))
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
                    this.currentStateMachines.Add(currentStateMachine);

                    this.StartStateMachine(currentStateMachine);
                }
            }
        }

        private void ProcessStopMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Processing Command {message.Type} Source {message.Source}");
            if (message.TargetBay == BayNumber.None)
            {
                if (this.currentStateMachines.Any(x => x.BayNumber == message.RequestingBay))
                {
                    message.TargetBay = message.RequestingBay;
                }
                else
                {
                    // message received from the UI: let's stop the first active FSM
                    if (this.currentStateMachines.Any())
                    {
                        message.TargetBay = this.currentStateMachines.First().BayNumber;
                    }
                }
            }

            if (this.currentStateMachines.Any(x => x.BayNumber == message.TargetBay))
            {
                foreach (var currentStateMachine in this.currentStateMachines.Where(x => x.BayNumber == message.TargetBay))
                {
                    if (message.Data is IStopMessageData data)
                    {
                        this.Logger.LogDebug($"Stop for {currentStateMachine.ToString()} with reason:{data.StopReason}");
                        currentStateMachine.Stop(data.StopReason);
                    }
                    else
                    {
                        currentStateMachine.Stop(StopRequestReason.Stop);
                    }
                }
            }
            else
            {
                var errorNotification = new NotificationMessage(
                    message.Data,
                    $"Bay {message.TargetBay} is already stopped",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    message.Type,
                    message.RequestingBay,
                    message.TargetBay,
                    MessageStatus.OperationEnd);

                this.Logger.LogTrace($"3:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.EventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
            }
        }

        private void ProcessStopTest(CommandMessage receivedMessage)
        {
            this.Logger.LogTrace("1:Method Start");

            // Check the stopTest message for the RepetitiveHorizontal state machine
            var stateMachines = this.currentStateMachines.Where(x => x.BayNumber == receivedMessage.TargetBay && x is RepetitiveHorizontalMovementsStateMachine);
            if (stateMachines.Any())
            {
                foreach (var fsm in stateMachines)
                {
                    var stateMachine = fsm as RepetitiveHorizontalMovementsStateMachine;
                    stateMachine.ProcessCommandMessage(receivedMessage);
                }

                return;
            }

            // Check the stopTest message for the HorizontalResolution state machine
            stateMachines = this.currentStateMachines.Where(x => x.BayNumber == receivedMessage.TargetBay && x is HorizontalResolutionStateMachine);
            if (stateMachines.Any())
            {
                foreach (var fsm in stateMachines)
                {
                    var stateMachine = fsm as HorizontalResolutionStateMachine;
                    stateMachine.ProcessCommandMessage(receivedMessage);
                }
                return;
            }

            // Check the stopTest message for the ProfileResolution state machine
            stateMachines = this.currentStateMachines.Where(x => x.BayNumber == receivedMessage.TargetBay && x is ProfileResolutionStateMachine);
            if (stateMachines.Any())
            {
                foreach (var fsm in stateMachines)
                {
                    var stateMachine = fsm as ProfileResolutionStateMachine;
                    stateMachine.ProcessCommandMessage(receivedMessage);
                }
                return;
            }

            // Check the stopTest message for the Positioning state machine
            stateMachines = this.currentStateMachines.Where(x => x.BayNumber == receivedMessage.RequestingBay && x is PositioningStateMachine);
            if (stateMachines.Any())
            {
                foreach (var fsm in stateMachines)
                {
                    var stateMachine = fsm as PositioningStateMachine;
                    stateMachine.ProcessCommandMessage(receivedMessage);
                }
            }
            //else
            //{
            //    this.Logger.LogDebug($"StopTest Message ignored, no active positioning state machine for bay {receivedMessage.TargetBay}");
            //}

            // Check the stopTest message for the ExtBayPositioning state machine
            stateMachines = this.currentStateMachines.Where(x => x.BayNumber == receivedMessage.RequestingBay && x is ExtBayPositioningStateMachine);
            if (stateMachines.Any())
            {
                foreach (var fsm in stateMachines)
                {
                    var stateMachine = fsm as ExtBayPositioningStateMachine;
                    stateMachine.ProcessCommandMessage(receivedMessage);
                }
            }
            else
            {
                this.Logger.LogDebug($"StopTest Message ignored, no active positioning state machine for bay {receivedMessage.TargetBay}");
            }
        }

        private void StartStateMachine(IStateMachine stateMachine)
        {
            if (stateMachine is null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

            try
            {
                stateMachine.Start();
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
