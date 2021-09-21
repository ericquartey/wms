using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateStartState : StateBase, IChangeRunningStateStartState, IStartMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private BayNumber currentBay;

        private bool requestedState;

        private StopRequestReason stopReason;

        #endregion

        #region Constructors

        public ChangeRunningStateStartState(
            IBaysDataProvider baysDataProvider,
            IMachineControlProvider machineControlProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));

            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"ChangeRunningStateStartState: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    this.requestedState = true;

                    this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MachineManager, commandMessage.RequestingBay);
                }
                else
                {
                    this.requestedState = false;

                    this.currentBay = this.baysDataProvider.GetAll().OrderBy(b => b.Number).First().Number;
                    this.stopReason = messageData.StopReason;
                    var newMessageData = new StopMessageData(this.stopReason);
                    this.machineControlProvider.StopOperation(newMessageData, this.currentBay, MessageActor.MachineManager, commandMessage.RequestingBay);
                    //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                    if (this.machineVolatileDataProvider.Mode != MachineMode.Shutdown)
                    {
                        this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(this.currentBay);
                        this.Logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
                    }
                }

                var notificationData = new ChangeRunningStateMessageData(
                    messageData.Enable,
                    machineData.FsmId,
                    messageData.CommandAction,
                    messageData.StopReason,
                    messageData.Verbosity);

                this.Message = new NotificationMessage(
                    notificationData,
                    $"Started Change Running State to {messageData.Enable}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.ChangeRunningState,
                    commandMessage.RequestingBay,
                    commandMessage.TargetBay,
                    MessageStatus.OperationStart);
            }
            else
            {
                var description = $"Power Enable Start State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.PowerStatusChangeStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        returnValue = this.requestedState ? this.GetState<IChangeRunningStateResetFaultState>() : this.GetState<IChangeRunningStateInverterPowerSwitch>();
                        break;

                    case MessageStatus.OperationError:
                    case MessageStatus.OperationFaultStop:
                        returnValue = this.GetState<IChangeRunningStateEndState>();

                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                        ((IEndState)returnValue).ErrorMessage = notification;
                        break;
                }
            }
            else
            {
                notificationStatus = this.machineControlProvider.StopOperationStatus(notification);
                if (notificationStatus != MessageStatus.NotSpecified)
                {
                    switch (notificationStatus)
                    {
                        case MessageStatus.OperationEnd:
                            this.UpdateResponseList(notificationStatus, notification.TargetBay);
                            var bays = this.baysDataProvider.GetAll().OrderBy(b => b.Number).ToList();
                            if (this.stateMachineResponses.Values.Count == bays.Count)
                            {
                                var messageData = new ChangeRunningStateMessageData(this.requestedState);
                                this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MachineManager, notification.RequestingBay);
                            }
                            else
                            {
                                this.currentBay = bays.FirstOrDefault(b => b.Number > this.currentBay)?.Number ?? BayNumber.None;
                                if (this.currentBay != BayNumber.None)
                                {
                                    var newMessageData = new StopMessageData(this.stopReason);
                                    this.machineControlProvider.StopOperation(newMessageData, this.currentBay, MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            break;

                        case MessageStatus.OperationError:
                            returnValue = this.GetState<IChangeRunningStateEndState>();

                            ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                            ((IEndState)returnValue).ErrorMessage = notification;
                            break;
                    }
                }
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"ChangeRunningStateStartState: OnStop.");
            var returnValue = this.GetState<IChangeRunningStateEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        private void UpdateResponseList(MessageStatus status, BayNumber targetBay)
        {
            if (this.stateMachineResponses.TryGetValue(targetBay, out var stateMachineResponse))
            {
                stateMachineResponse = status;
                this.stateMachineResponses[targetBay] = stateMachineResponse;
            }
            else
            {
                this.stateMachineResponses.Add(targetBay, status);
            }
        }

        #endregion
    }
}
