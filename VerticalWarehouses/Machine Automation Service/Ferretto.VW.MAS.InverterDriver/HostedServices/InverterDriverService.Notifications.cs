using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming;
using Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Statistics;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    internal partial class InverterDriverService
    {
        #region Methods

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
            this.currentStateMachines.TryGetValue(inverterIndex, out var messageCurrentStateMachine);

            switch (receivedMessage.Type)
            {
                case FieldMessageType.DataLayerReady:
                    // start communication
                    await this.StartHardwareCommunicationsAsync(serviceProvider);
                    this.InitializeTimers(serviceProvider);

                    break;

                case FieldMessageType.Positioning:
                    // close state machine
                    {
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError ||
                            receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            this.Logger.LogTrace($"4:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] count {this.currentStateMachines.Count}");

                            if (messageCurrentStateMachine is PositioningStateMachine)
                            {
                                this.currentStateMachines.Remove(inverterIndex);
                            }
                            else if (messageCurrentStateMachine is PositioningTableStateMachine currentPositioning)
                            {
                                this.dataOld = currentPositioning.data;
                                this.currentStateMachines.Remove(inverterIndex);
                            }
                            else
                            {
                                this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                            }

                            this.Logger.LogTrace("4: Stop the timer for update shaft position");
                            this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(100, 10000);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);

                            this.Logger.LogDebug($"4b: currentStateMachines count {this.currentStateMachines.Count}");
                        }

                        break;
                    }
                case FieldMessageType.CalibrateAxis:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");

                        if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                        {
                            this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                            this.currentStateMachines.Remove(inverterIndex);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.ShutterPositioning:
                    // close state machine

                    this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterStop:
                    // close state machine

                    this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

                        if (messageCurrentStateMachine is SwitchOnStateMachine || messageCurrentStateMachine is StopStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already switched on / stopped current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType().Name} Handling {receivedMessage.Type}");
                        }
                        this.Logger.LogTrace("Stop the timer for update status word");
                        this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                    }

                    break;

                case FieldMessageType.InverterSwitchOff:
                    // close state machine
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");

                        if (messageCurrentStateMachine is SwitchOffStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already switched off current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType().Name} Handling {receivedMessage.Type}");
                        }

                        this.Logger.LogTrace("Stop the timer for update status word");
                        this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);

                        var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOn:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");

                        if (messageCurrentStateMachine is PowerOnStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already powered on current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType().Name} Handling {receivedMessage.Type}");
                        }
                        this.Logger.LogTrace("Stop the timer for update status word");
                        this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);

                        var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOff:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");

                        if (messageCurrentStateMachine is PowerOffStateMachine)
                        {
                            var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                            var inverter = invertersProvider.GetByIndex(inverterIndex);
                            if (inverter is AngInverterStatus || inverter is AcuInverterStatus)
                            {
                                this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(10000, 10000);
                            }

                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already powered off current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType().Name} Handling {receivedMessage.Type}");
                        }
                        this.Logger.LogTrace("Stop the timer for update status word");
                        this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);

                        var nextMessage = ((InverterPowerOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterFaultReset:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type}), inverter {inverterIndex}");

                        if (messageCurrentStateMachine is ResetFaultStateMachine)
                        {
                            var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                            var inverter = invertersProvider.GetByIndex(inverterIndex);
                            if (inverter is AngInverterStatus || inverter is AcuInverterStatus)
                            {
                                this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(1000, 10000);
                            }
                            this.currentStateMachines.Remove(inverterIndex);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterProgramming:
                case FieldMessageType.InverterReading:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError ||
                            receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is InverterProgrammigState ||
                            messageCurrentStateMachine is InverterReadingState)
                        {
                            this.currentStateMachines.Remove(inverterIndex);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterStatistics:
                    // close state machine

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError ||
                            receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is StatisticsStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);

                            this.Logger.LogTrace("Stop the timer for update status word");
                            this.statusWordUpdateTimer[(int)inverterIndex]?.Change(100, 10000);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType().Name} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                // TODO - Only for Bender
                case FieldMessageType.SensorsChanged:
                    if (receivedMessage.Data is ISensorsChangedFieldMessageData dataIOs)
                    {
                        var ioIndex = receivedMessage.DeviceIndex;
                        if (ioIndex == 0)
                        {
                            var ioStatuses = new bool[8];
                            ioStatuses[7] = (!dataIOs.SensorsStates[(int)IOMachineSensors.LuPresentInMachineSide] && !dataIOs.SensorsStates[(int)IOMachineSensors.LuPresentInOperatorSide]);

                            var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                            var inverter = invertersProvider.GetByIndex(InverterIndex.MainInverter);
                            if (inverter is AngInverterStatus angInverter)
                            {
                                ioStatuses[5] = angInverter.ANG_ElevatorOverrunSensor;
                                if (angInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish[(int)InverterIndex.MainInverter])
                                {
                                    this.Logger.LogTrace("Sensor Update");

                                    var msgNotification = new FieldNotificationMessage(
                                        new InverterStatusUpdateFieldMessageData(angInverter.Inputs),
                                        "Inverter Inputs update",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.InverterStatusUpdate,
                                        MessageStatus.OperationExecuting,
                                        angInverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                            }
                        }
                    }
                    break;
            }

            if (receivedMessage.Source == FieldMessageActor.InverterDriver
                && (receivedMessage.Status == MessageStatus.OperationEnd
                    || receivedMessage.Status == MessageStatus.OperationStop
                    )
                )
            {
                var notificationMessageToFsm = receivedMessage;

                // forward the message to upper level
                notificationMessageToFsm.Destination = FieldMessageActor.DeviceManager;

                this.eventAggregator?
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(notificationMessageToFsm);
            }
        }

        #endregion
    }
}
