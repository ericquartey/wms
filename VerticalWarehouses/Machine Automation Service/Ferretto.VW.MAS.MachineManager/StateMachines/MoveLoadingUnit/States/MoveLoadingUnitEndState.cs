using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitEndState : StateBase, IMoveLoadingUnitEndState, IEndState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitEndState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IBaysDataProvider baysDataProvider,
            IMissionsDataProvider missionsDataProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));

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

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, request reason {this.StopRequestReason}, {commandMessage.Description}");
            this.EndMessage = new NotificationMessage(
                commandMessage.Data,
                commandMessage.Description,
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                commandMessage.Type,
                commandMessage.RequestingBay,
                commandMessage.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.StopRequestReason));

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
            }
            if (this.StopRequestReason == StopRequestReason.NoReason)
            {
                this.IsCompleted = true;
                this.missionsDataProvider.Delete(this.mission?.Id);
            }
            else
            {
                var newMessageData = new StopMessageData(this.StopRequestReason);
                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
                this.mission.FsmStateName = this.GetType().Name;
                this.mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.mission);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.StopOperationStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        break;
                }

                if (this.stateMachineResponses.Values.Count == this.baysDataProvider.GetAll().Count())
                {
                    this.IsCompleted = true;
                    this.missionsDataProvider.Delete(this.mission?.Id);
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
