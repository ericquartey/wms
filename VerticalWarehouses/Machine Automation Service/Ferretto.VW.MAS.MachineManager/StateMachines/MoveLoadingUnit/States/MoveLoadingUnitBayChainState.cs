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
    internal class MoveLoadingUnitBayChainState : StateBase, IMoveLoadingUnitBayChainState, IProgressMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private IMoveLoadingUnitMessageData messageData;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitBayChainState(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
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
                this.mission.FsmStateName = nameof(MoveLoadingUnitBayChainState);
                this.missionsDataProvider.Update(this.mission);

                var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitDestination);
                if (bay is null)
                {
                    var description = $"{this.GetType().Name}: destination bay not found {moveData.LoadingUnitDestination}";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }
                if (this.loadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number))
                {
                    this.mission.RestoreConditions = false;
                }
                if (!this.loadingUnitMovementProvider.MoveCarousel(moveData.LoadingUnitId, MessageActor.MachineManager, bay.Number, this.mission.RestoreConditions))
                {
                    var description = $"MoveLoadingUnitBayChainState: Move Bay chain not possible in bay {bay.Number}. Wait for resume";
                    // we don't want an exception here because this is the normal procedure:
                    // send a second LU in lower position while operator is working on upper position
                    this.Logger.LogDebug(description);
                }

                moveData.Status = MissionStatus.Waiting;
                this.missionsDataProvider.Update(this.mission);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.CarouselStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.RequestingBay == this.mission.TargetBay)
                    {
                        if (notification.Type == MessageType.Positioning)
                        {
                            var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                            if (destination is null)
                            {
                                throw new StateMachineException($"Upper position not defined for bay {bay.Number}", null, MessageActor.MachineManager);
                            }
                            this.mission.LoadingUnitDestination = destination.Location;

                            var newMessageData = new MoveLoadingUnitMessageData(
                                this.mission.MissionType,
                                this.mission.LoadingUnitSource,
                                this.mission.LoadingUnitDestination,
                                this.mission.LoadingUnitCellSourceId,
                                this.mission.DestinationCellId,
                                this.mission.LoadingUnitId,
                                (this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                                false,
                                this.mission.FsmId,
                                this.messageData.CommandAction,
                                this.messageData.StopReason,
                                this.messageData.Verbosity);

                            this.Message = new NotificationMessage(
                                newMessageData,
                                $"Loading Unit {this.mission.LoadingUnitId} placed on bay {bay.Number}",
                                MessageActor.AutomationService,
                                MessageActor.MachineManager,
                                MessageType.MoveLoadingUnit,
                                notification.RequestingBay,
                                bay.Number,
                                MessageStatus.OperationWaitResume);

                            if (this.mission.RestoreConditions)
                            {
                                this.loadingUnitMovementProvider.UpdateLastBayChainPosition(this.mission.TargetBay);
                                this.mission.RestoreConditions = false;
                            }
                            if (this.mission.NeedHomingAxis == Axis.BayChain)
                            {
                                this.Logger.LogDebug($"Homing Bay occupied start");
                                this.loadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.mission.LoadingUnitId, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                returnValue = this.BayChainEnd();
                            }
                        }
                        else if (notification.Type == MessageType.Homing)
                        {
                            returnValue = this.BayChainEnd();
                        }
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

        private IState BayChainEnd()
        {
            IState returnValue;
            if (this.mission.WmsId.HasValue)
            {
                returnValue = this.GetState<IMoveLoadingUnitWaitPickConfirm>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitWaitEjectConfirm>();
            }

            return returnValue;
        }

        protected override IState OnResume(CommandMessage commandMessage)
        {
            IState returnValue = this;
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.mission.LoadingUnitDestination))
#endif
            {
                if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
                {
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                    if (!this.loadingUnitMovementProvider.MoveCarousel(this.mission.LoadingUnitId, MessageActor.MachineManager, bay.Number, false))
                    {
                        var description = $"MoveLoadingUnitBayChainState: Move Bay chain not possible in bay {bay.Number}. Wait for another resume";
                        throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                    }
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.mission.TargetBay);
            }
#endif
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
