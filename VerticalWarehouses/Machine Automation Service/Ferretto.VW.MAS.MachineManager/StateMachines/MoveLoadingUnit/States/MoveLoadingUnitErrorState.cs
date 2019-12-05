using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitErrorState : StateBase, IMoveLoadingUnitErrorState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitErrorState(
            IBaysDataProvider baysDataProvider,
            IMissionsDataProvider missionsDataProvider,
            ICellsProvider cellsProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMachineProvider machineProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
            }

            var newMessageData = new StopMessageData(StopRequestReason.Error);
            this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
            this.mission.FsmStateName = this.GetType().Name;
            this.missionsDataProvider.Update(this.mission);
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
                    // TODO remove stop mission
                    {
                        // stop mission
                        this.mission.FsmRestoreStateName = null;
                        returnValue = this.GetState<IMoveLoadingUnitEndState>();
                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
                    }
                    //{
                    //    // wait for resume or stop
                    //    this.mission.RestoreConditions = true;
                    //    this.missionsDataProvider.Update(this.mission);
                    //}
                }
            }

            return returnValue;
        }

        protected override IState OnResume(CommandMessage commandMessage)
        {
            IState returnValue = this;
            // TODO try to continue the mission from where it stopped
            switch (this.mission.FsmRestoreStateName)
            {
                case nameof(MoveLoadingUnitCloseShutterState):
                    break;

                case nameof(MoveLoadingUnitDepositUnitState):
                    break;

                case nameof(MoveLoadingUnitLoadElevatorState):
                    break;

                case nameof(MoveLoadingUnitMoveToTargetState):
                    break;

                case nameof(MoveLoadingUnitStartState):
                    break;

                case nameof(MoveLoadingUnitWaitEjectConfirm):
                    break;

                case nameof(MoveLoadingUnitWaitPickConfirm):
                    break;

                default:
                    this.Logger.LogError($"OnResume: no valid FsmRestoreStateName {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
                    break;
            }
            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

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
