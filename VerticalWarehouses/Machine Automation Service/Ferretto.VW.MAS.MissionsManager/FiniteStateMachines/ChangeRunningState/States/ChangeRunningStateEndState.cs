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
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateEndState : StateBase, IChangeRunningStateEndState, IEndState
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();

        #endregion

        #region Constructors

        public ChangeRunningStateEndState(
            IBaysProvider baysProvider,
            IMachineControlProvider machineControlProvider,
            IErrorsProvider errorsProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage EndMessage { get; set; }

        public NotificationMessage ErrorMessage { get; set; }

        public bool IsCompleted { get; set; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            this.EndMessage = new NotificationMessage(
                commandMessage.Data,
                commandMessage.Description,
                MessageActor.AutomationService,
                MessageActor.MissionsManager,
                commandMessage.Type,
                commandMessage.RequestingBay,
                commandMessage.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.StopRequestReason));

            if (this.StopRequestReason == StopRequestReason.NoReason)
            {
                this.IsCompleted = true;
                if (this.EndMessage.Data is IChangeRunningStateMessageData runningState && runningState.Enable)
                {
                    this.errorsProvider.ResolveAll();
                }
            }
            else
            {
                this.errorsProvider.RecordNew(MachineErrors.ConditionsNotMetForRunning, commandMessage.RequestingBay);

                var newMessageData = new StopMessageData(this.StopRequestReason);
                this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.StopOperationStatus(notification);

            if (notificationStatus != MessageStatus.NoStatus)
            {
                switch (notificationStatus)
                {
                    // STate machine is in error, any response from device manager state machines will do to complete state machine shutdown
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationEnd:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        break;
                }

                if (this.stateMachineResponses.Values.Count == this.baysProvider.GetAll().Count())
                {
                    this.IsCompleted = true;
                }
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
