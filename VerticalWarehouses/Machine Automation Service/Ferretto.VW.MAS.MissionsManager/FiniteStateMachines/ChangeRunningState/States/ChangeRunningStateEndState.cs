using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Providers.Interfaces;
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

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        #endregion

        #region Constructors

        public ChangeRunningStateEndState(
            IBaysProvider baysProvider,
            IMachineControlProvider machineControlProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));

            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
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
            if (this.StopRequestReason == StopRequestReason.NoReason)
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

                this.IsCompleted = true;
            }
            else
            {
                var newMessageData = new StopMessageData(this.StopRequestReason);
                this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.PowerStatusChangeStatus(notification);

            if (notificationStatus != MessageStatus.NoStatus)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        break;

                    case MessageStatus.OperationError:
                        break;
                }

                if (this.stateMachineResponses.Values.Count == this.baysProvider.GetAll().Count())
                {
                    this.IsCompleted = true;
                }
            }

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
