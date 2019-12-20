using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable LocalVariableHidesMember
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitCloseShutterState : StateBase, IMoveLoadingUnitCloseShutterState, IProgressMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private IMoveLoadingUnitMessageData messageData;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitCloseShutterState(
            IBaysDataProvider baysDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData
                && machineData is Mission moveData
                )
            {
                this.Logger.LogDebug($"{this.GetType().Name}: {moveData}");
                this.messageData = messageData;
                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitCloseShutterState);
                this.missionsDataProvider.Update(this.mission);

                var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitDestination);
                if (bay is null)
                {
                    var description = $"{this.GetType().Name}: destination bay not found {moveData.LoadingUnitDestination}";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }
                this.loadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, moveData.RestoreConditions);

                moveData.RestoreConditions = false;
                this.missionsDataProvider.Update(this.mission);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    bool isEject = this.mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                        && this.mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                        && this.mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                        && this.mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;
                    if (isEject)
                    {
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                        var messageData = new MoveLoadingUnitMessageData(
                            this.mission.MissionType,
                            this.mission.LoadingUnitSource,
                            this.mission.LoadingUnitDestination,
                            this.mission.LoadingUnitCellSourceId,
                            this.mission.DestinationCellId,
                            this.mission.LoadingUnitId,
                            (this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                            isEject,
                            this.mission.FsmId,
                            this.messageData.CommandAction,
                            this.messageData.StopReason,
                            this.messageData.Verbosity);

                        this.Message = new NotificationMessage(
                            messageData,
                            $"Loading Unit {this.mission.LoadingUnitId} placed on bay {bay.Number}",
                            MessageActor.AutomationService,
                            MessageActor.MachineManager,
                            MessageType.MoveLoadingUnit,
                            notification.RequestingBay,
                            bay.Number,
                            MessageStatus.OperationWaitResume);

                        if (this.mission.WmsId.HasValue)
                        {
                            if (bay.Positions.Count() == 1
                                || bay.Positions.FirstOrDefault(x => x.Location == this.mission.LoadingUnitDestination).IsUpper
                                || bay.Carousel is null)
                            {
                                returnValue = this.GetState<IMoveLoadingUnitWaitPickConfirm>();
                            }
                            else
                            {
                                returnValue = this.GetState<IMoveLoadingUnitBayChainState>();
                            }
                        }
                        else if (bay.Number == notification.RequestingBay)
                        {
                            returnValue = this.GetState<IMoveLoadingUnitWaitEjectConfirm>();
                        }
                        else
                        {
                            returnValue = this.GetState<IMoveLoadingUnitEndState>();
                        }
                    }
                    else
                    {
                        returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.OnStop(StopRequestReason.Error);
                    if (returnValue is IEndState endState)
                    {
                        endState.ErrorMessage = notification;
                    }
                    break;
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            IState returnValue;
            if (this.mission != null
                && this.mission.IsRestoringType()
                )
            {
                this.mission.FsmRestoreStateName = this.mission.FsmStateName;
                returnValue = this.GetState<IMoveLoadingUnitErrorState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            if (returnValue is IEndState endState)
            {
                endState.StopRequestReason = reason;
            }

            return returnValue;
        }

        #endregion
    }
}
