﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateEndState : StateBase, IChangeRunningStateEndState, IEndState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly IMachineVolatileDataProvider machineModeDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();

        #endregion

        #region Constructors

        public ChangeRunningStateEndState(
            IBaysDataProvider baysDataProvider,
            IMachineControlProvider machineControlProvider,
            IErrorsProvider errorsProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISensorsProvider sensorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.machineModeDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage EndMessage { get; set; }

        public NotificationMessage ErrorMessage { get; set; }

        public bool IsCompleted { get; set; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"ChangeRunningStateEndState: received command {commandMessage.Type}, {commandMessage.Description}, reason {this.StopRequestReason}");
            this.EndMessage = new NotificationMessage(
                commandMessage.Data,
                commandMessage.Description,
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                commandMessage.Type,
                commandMessage.RequestingBay,
                commandMessage.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.StopRequestReason));

            if (this.EndMessage.Data is IChangeRunningStateMessageData runningState)
            {
                if (this.StopRequestReason is StopRequestReason.NoReason
                    && this.sensorsProvider.IsMachineSecurityRunning == runningState.Enable)
                {
                    this.IsCompleted = true;
                    if (runningState.Enable)
                    {
                        this.errorsProvider.ResolveAll();
                    }

                    this.machineModeDataProvider.Mode = MachineMode.Manual;
                    this.Logger.LogInformation($"Machine status switched to {this.machineModeDataProvider.Mode}; Running state {runningState.Enable}");
                }
                else
                {
                    var endMessageData = new ChangeRunningStateMessageData(false, null, runningState.CommandAction, StopRequestReason.Error);
                    this.EndMessage = new NotificationMessage(
                        endMessageData,
                        commandMessage.Description,
                        MessageActor.AutomationService,
                        MessageActor.MachineManager,
                        commandMessage.Type,
                        commandMessage.RequestingBay,
                        commandMessage.TargetBay,
                        StopRequestReasonConverter.GetMessageStatusFromReason(StopRequestReason.Error));

                    this.errorsProvider.RecordNew(MachineErrorCode.ConditionsNotMetForRunning, commandMessage.RequestingBay);

                    {
                        var newMessageData = new ChangeRunningStateMessageData(false);

                        this.machineControlProvider.StartChangePowerStatus(newMessageData, MessageActor.MachineManager, commandMessage.RequestingBay);
                    }

                    {
                        var newMessageData = new StopMessageData(this.StopRequestReason);
                        this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
                    }
                }
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
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
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
                        // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                        case MessageStatus.OperationError:
                        case MessageStatus.OperationEnd:
                            this.UpdateResponseList(notificationStatus, notification.TargetBay);
                            break;
                    }
                }
            }
            if (this.stateMachineResponses.Values.Count == this.baysDataProvider.GetAll().Count())
            {
                this.IsCompleted = true;
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            return this;
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
