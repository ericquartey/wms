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
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateInverterPowerSwitch : StateBase, IChangeRunningStateInverterPowerSwitch
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private BayNumber currentBay;

        private bool enable;

        #endregion

        #region Constructors

        public ChangeRunningStateInverterPowerSwitch(
            IBaysDataProvider baysDataProvider,
            IMachineControlProvider machineControlProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));

            this.machineControlProvider = machineControlProvider ??
                throw new ArgumentNullException(nameof(machineControlProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"ChangeRunningStateInverterPowerSwitch: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                this.currentBay = this.baysDataProvider.GetBayNumbers().ToArray().First();
                this.enable = messageData.Enable;
                var inverterMessageData = new InverterPowerEnableMessageData(this.enable);
                this.machineControlProvider.StartInverterPowerChange(inverterMessageData, this.currentBay, MessageActor.MachineManager, commandMessage.RequestingBay);
            }
            else
            {
                var description = $"Power Enable Reset Fault State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.InverterPowerChangeStatus(notification);
            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        var bays = this.baysDataProvider.GetAll();

                        if (this.stateMachineResponses.Values.Count == bays.Count())
                        {
                            returnValue = this.GetState<IChangeRunningStateEndState>();
                            ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
                        }
                        else
                        {
                            this.currentBay = bays.FirstOrDefault(b => b.Number > this.currentBay)?.Number ?? BayNumber.None;
                            if (this.currentBay != BayNumber.None)
                            {
                                var inverterMessageData = new InverterPowerEnableMessageData(this.enable);
                                this.machineControlProvider.StartInverterPowerChange(inverterMessageData, this.currentBay, MessageActor.MachineManager, notification.RequestingBay);
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
