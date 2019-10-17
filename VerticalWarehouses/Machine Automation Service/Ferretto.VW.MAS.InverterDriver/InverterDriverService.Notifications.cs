﻿using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.InverterDriver
{
    partial class InverterDriverService
    {
        #region Methods

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
            this.currentStateMachines.TryGetValue(inverterIndex, out var messageCurrentStateMachine);

            switch (receivedMessage.Type)
            {
                case FieldMessageType.DataLayerReady:

                    await this.StartHardwareCommunications(serviceProvider);
                    this.InitializeTimers();

                    break;

                case FieldMessageType.Positioning:
                    {
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError ||
                            receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            this.Logger.LogDebug($"4:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] count {this.currentStateMachines.Count}");

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
                                this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            this.Logger.LogTrace("4: Stop the timer for update shaft position");
                            this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(100, 1000);

                            this.Logger.LogDebug($"4b: currentStateMachines count {this.currentStateMachines.Count}");
                        }

                        break;
                    }
                case FieldMessageType.CalibrateAxis:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

                        if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                        {
                            this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(100, 1000);
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.ShutterPositioning:

                    this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate [{messageCurrentStateMachine?.GetType()}] Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterStop:

                    this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");
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
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType()} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

                        if (messageCurrentStateMachine is SwitchOffStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already switched off current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOn:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

                        if (messageCurrentStateMachine is PowerOnStateMachine)
                        {
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        // If inverter is already powered on current state machine is null but end notification is still sent so no error to report here
                        else if (messageCurrentStateMachine != null)
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOff:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogDebug($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

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
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterPowerOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.EnqueueCommand(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterFaultReset:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.Logger.LogTrace($"Deallocating [{messageCurrentStateMachine?.GetType().Name}] state machine ({receivedMessage.Type})");

                        if (messageCurrentStateMachine is ResetFaultStateMachine)
                        {
                            var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                            var inverter = invertersProvider.GetByIndex(inverterIndex);
                            if (inverter is AngInverterStatus || inverter is AcuInverterStatus)
                            {
                                this.axisPositionUpdateTimer[(int)inverterIndex]?.Change(1000, 1000);
                            }
                            this.currentStateMachines.Remove(inverterIndex);
                        }
                        else
                        {
                            this.Logger.LogError($"Failed to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }

                    break;
            }

            if (receivedMessage.Source == FieldMessageActor.InverterDriver)
            {
                if (receivedMessage.Status == MessageStatus.OperationEnd ||
                    receivedMessage.Status == MessageStatus.OperationStop)
                {
                    var notificationMessageToFsm = receivedMessage;
                    //TEMP Set the destination of message to FSM
                    notificationMessageToFsm.Destination = FieldMessageActor.FiniteStateMachines;

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessageToFsm);
                }
            }
        }

        #endregion
    }
}
