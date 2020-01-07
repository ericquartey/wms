using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveErrorState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveErrorState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.cellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.elevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            this.Mission.FsmStateName = nameof(MissionMoveErrorState);
            this.missionsDataProvider.Update(this.Mission);

            var newMessageData = new StopMessageData(StopRequestReason.Error);
            this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
            this.Mission.RestoreConditions = false;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.missionsDataProvider.Update(this.Mission);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            if (this.Mission.ErrorMovements != MissionErrorMovements.None)
            {
                var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            this.logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning end");
                            var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                if (this.Mission.FsmRestoreStateName != nameof(MissionMoveCloseShutterState))
                                {
                                    if (this.Mission.NeedMovingBackward)
                                    {
                                        if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                                        {
                                            this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                                        }
                                    }
                                    else
                                    {
                                        var isLoaded = (this.Mission.FsmRestoreStateName == nameof(MissionMoveDepositUnitState));
                                        var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
                                        if (this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, isLoaded, measure, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                                        {
                                            this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                                        }
                                    }
                                }
                                if ((this.Mission.ErrorMovements & (MissionErrorMovements.MoveForward | MissionErrorMovements.MoveBackward)) == 0)
                                {
                                    this.RestoreOriginalStep();
                                }
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveShutter;
                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                            }
                        }
                        else
                        {
                            this.logger.LogDebug($"{this.GetType().Name}: Manual Horizontal positioning end");
                            if ((this.Mission.ErrorMovements & MissionErrorMovements.MoveBackward) != 0)
                            {
                                this.Mission.NeedMovingBackward = false;
                                if (this.Mission.NeedHomingAxis == Axis.Horizontal
                                    && this.Mission.FsmRestoreStateName == nameof(MissionMoveDepositUnitState)
                                    )
                                {
                                    this.Mission.FsmRestoreStateName = null;
                                    var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                                    newStep.OnEnter(null);
                                }
                                else
                                {
                                    this.RestoreOriginalStep();
                                }
                            }
                            else
                            {
                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveForward;
                                this.loadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                                if (this.Mission.FsmRestoreStateName == nameof(MissionMoveLoadElevatorState))
                                {
                                    this.RestoreLoadElevatorEnd();
                                }
                                else
                                {
                                    this.RestoreDepositEnd();
                                }
                            }
                        }
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            this.Mission.ErrorMovements = MissionErrorMovements.None;
                            var newMessageData = new StopMessageData(StopRequestReason.Error);
                            this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                        }
                        break;
                }
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.logger.LogDebug($"{this.GetType().Name}: Resume mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, from {this.Mission.FsmRestoreStateName}, loadUnit {this.Mission.LoadingUnitId}");

            switch (this.Mission.FsmRestoreStateName)
            {
                case nameof(MissionMoveBayChainState):
                    this.RestoreBayChain();
                    break;

                case nameof(MissionMoveCloseShutterState):
                    this.RestoreCloseShutter();
                    break;

                case nameof(MissionMoveDepositUnitState):
                    this.RestoreDepositStart();
                    break;

                case nameof(MissionMoveEndState):
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(command);
                    }
                    break;

                case nameof(MissionMoveLoadElevatorState):
                    this.RestoreLoadElevatorStart();
                    break;

                case nameof(MissionMoveToTargetState):
                    this.Mission.RestoreConditions = true;
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(command);
                    }
                    break;

                case nameof(MissionMoveStartState):
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(command);
                    }
                    break;

                case nameof(MissionMoveWaitPickState):
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveWaitPickState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(command);
                    }
                    break;

                default:
                    this.logger.LogError($"{this.GetType().Name}: no valid FsmRestoreStateName {this.Mission.FsmRestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadingUnitId}");

                    {
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;
            }
        }

        private void RestoreBayChain()
        {
            if (this.loadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.Mission.TargetBay))
            {
                // movement is finished
                var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                if (destination is null)
                {
                    throw new StateMachineException($"Upper position not defined for bay {bay.Number}", null, MessageActor.MachineManager);
                }
                this.Mission.LoadingUnitDestination = destination.Location;

                var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                {
                    this.baysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadingUnitId);
                    this.baysDataProvider.SetLoadingUnit(origin.Id, null);
                    transaction.Commit();
                }

                var newMessageData = new MoveLoadingUnitMessageData(
                    this.Mission.MissionType,
                    this.Mission.LoadingUnitSource,
                    this.Mission.LoadingUnitDestination,
                    this.Mission.LoadingUnitCellSourceId,
                    this.Mission.DestinationCellId,
                    this.Mission.LoadingUnitId,
                    (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                    false,
                    this.Mission.FsmId);

                var msg = new NotificationMessage(
                    newMessageData,
                    $"Loading Unit {this.Mission.LoadingUnitId} placed on bay {bay.Number}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.MoveLoadingUnit,
                    this.Mission.TargetBay,
                    bay.Number,
                    MessageStatus.OperationWaitResume);
                this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                this.Mission.FsmRestoreStateName = null;
                this.Mission.RestoreConditions = false;
                this.Mission.NeedMovingBackward = false;
                if (this.Mission.WmsId.HasValue)
                {
                    var newStep = new MissionMoveWaitPickState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.FsmRestoreStateName = null;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveBayChainState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreCloseShutter()
        {
            var shutterPosition = this.sensorsProvider.GetShutterPosition(this.Mission.TargetBay);
            if (shutterPosition != ShutterPosition.Opened)
            {
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutter;
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.FsmRestoreStateName = null;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreDepositEnd()
        {
            bool bayShutter = false;
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(null);

                if (this.Mission.LoadingUnitDestination is LoadingUnitLocation.Cell)
                {
                    var destinationCellId = this.Mission.DestinationCellId;
                    if (destinationCellId.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(destinationCellId.Value, this.Mission.LoadingUnitId);
                    }
                    else
                    {
                        throw new InvalidOperationException("Loading unit movement to target cell has no target cell specified.");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.Mission.LoadingUnitDestination);
                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadingUnitId);
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    bayShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                }

                transaction.Commit();
            }

            var msg = new NotificationMessage(
                            null,
                            $"Load Unit position changed",
                            MessageActor.Any,
                            MessageActor.MachineManager,
                            MessageType.Positioning,
                            this.Mission.TargetBay,
                            this.Mission.TargetBay,
                            MessageStatus.OperationUpdateData);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            this.Mission.FsmRestoreStateName = null;
            this.Mission.NeedMovingBackward = false;
            if (bayShutter)
            {
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        /// <summary>
        /// we try to resolve different error situations:
        /// 1)  if the elevator has moved vertically we can start from previous step: if elevator is not full will be checked later
        /// 2)  if we have to deposit in cell we try to move chain forward in manual mode calculating the distance. Then repeat deposit step
        /// 3)  if we have to deposit in bay we have more cases:
        ///     3.1) shutter is open: try to move forward in manual mode calculating the distance
        ///     3.2) shutter is closed or intermediate: open the shutter and move chain back. Then repeat deposit step
        /// </summary>
        /// <returns></returns>
        private void RestoreDepositStart()
        {
            var destination = this.loadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!destination.HasValue || Math.Abs((destination.Value - current)) > 2))
            {
                if (this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.FsmRestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadingUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    throw new StateMachineException($"Impossible to restore mission for LoadUnit {this.Mission.LoadingUnitId}");
                }
            }

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            switch (this.Mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.Mission.DestinationCellId.Value);
                        // invert direction?
                        if (this.Mission.NeedMovingBackward)
                        {
                            this.Mission.Direction = (cell.Side != WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }
                        else
                        {
                            this.Mission.Direction = (cell.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    // invert direction?
                    if (this.Mission.NeedMovingBackward)
                    {
                        this.Mission.Direction = (bay.Side != WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    else
                    {
                        this.Mission.Direction = (bay.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    this.Mission.OpenShutterPosition = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadingUnitDestination);
                    var shutterPosition = this.sensorsProvider.GetShutterPosition(this.Mission.TargetBay);
                    if (this.Mission.OpenShutterPosition == ShutterPosition.Half && shutterPosition == ShutterPosition.Opened)
                    {
                        this.Mission.OpenShutterPosition = shutterPosition;
                    }
                    if (shutterPosition != this.Mission.OpenShutterPosition)
                    {
                        this.logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                        this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutter;
                        return;
                    }
                    break;
            }
            if (this.Mission.NeedMovingBackward
                || this.Mission.NeedHomingAxis == Axis.Horizontal
                )
            {
                this.Mission.NeedMovingBackward = true;
                this.logger.LogDebug($"{this.GetType().Name}: Manual Horizontal back positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
            }
            else
            {
                this.logger.LogDebug($"{this.GetType().Name}: Manual Horizontal forward positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if ((this.Mission.ErrorMovements & (MissionErrorMovements.MoveForward | MissionErrorMovements.MoveBackward)) == 0)
            {
                this.RestoreOriginalStep();
            }
        }

        private void RestoreLoadElevatorEnd()
        {
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(this.Mission.LoadingUnitId);

                if (this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell)
                {
                    var sourceCellId = this.Mission.LoadingUnitCellSourceId;
                    if (sourceCellId.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(sourceCellId.Value, null);
                    }
                    else
                    {
                        throw new InvalidOperationException("");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.Mission.LoadingUnitSource);
                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                }

                transaction.Commit();
            }

            // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
            if (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitId > 0
                )
            {
                try
                {
                    this.Mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.Mission.LoadingUnitId);
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.errorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull);
                    this.Mission.LoadingUnitDestination = this.Mission.LoadingUnitSource;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    return;
                }
            }

            var msg = new NotificationMessage(
                            null,
                            $"Load Unit position changed",
                            MessageActor.Any,
                            MessageActor.MachineManager,
                            MessageType.Positioning,
                            this.Mission.TargetBay,
                            this.Mission.TargetBay,
                            MessageStatus.OperationUpdateData);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            this.Mission.FsmRestoreStateName = null;
            this.Mission.NeedMovingBackward = false;
            {
                var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        /// <summary>
        /// we try to resolve different error situations:
        /// 1)  if the elevator has moved vertically we can start from previous step: if elevator is not full will be checked later
        /// 2)  if we have to load from cell we move chain back in manual mode (it is safer to have elevator empty). Then repeat loading step
        /// 3)  if we have to load from bay we have more cases:
        ///     3.1) shutter is open: try to move chain back in manual mode (profile measure is not possible in manual mode). Then repeat loading step
        ///     3.2) shutter is closed or intermediate: open the shutter and start from 3.1)
        /// </summary>
        /// <returns></returns>
        private void RestoreLoadElevatorStart()
        {
            var origin = this.loadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!origin.HasValue || Math.Abs((origin.Value - current)) > 2))
            {
                if (!this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.FsmRestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadingUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.FsmRestoreStateName = null;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);

                    return;
                }
                else
                {
                    throw new StateMachineException($"Impossible to restore mission for LoadUnit {this.Mission.LoadingUnitId}");
                }
            }

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            switch (this.Mission.LoadingUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.Mission.LoadingUnitCellSourceId.Value);

                        // invert direction?
                        if (this.Mission.NeedMovingBackward)
                        {
                            this.Mission.Direction = cell.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }
                        else
                        {
                            this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitSource);
                    // invert direction?
                    if (this.Mission.NeedMovingBackward)
                    {
                        this.Mission.Direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    else
                    {
                        this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    this.Mission.OpenShutterPosition = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadingUnitSource);
                    var shutterPosition = this.sensorsProvider.GetShutterPosition(this.Mission.TargetBay);
                    if (this.Mission.OpenShutterPosition == ShutterPosition.Half && shutterPosition == ShutterPosition.Opened)
                    {
                        this.Mission.OpenShutterPosition = shutterPosition;
                    }
                    if (shutterPosition != this.Mission.OpenShutterPosition)
                    {
                        this.logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                        this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutter;
                        return;
                    }
                    break;
            }
            if (this.Mission.NeedMovingBackward)
            {
                this.logger.LogDebug($"{this.GetType().Name}: Manual Horizontal back positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
            }
            else
            {
                this.logger.LogDebug($"{this.GetType().Name}: Manual Horizontal forward positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if ((this.Mission.ErrorMovements & (MissionErrorMovements.MoveForward | MissionErrorMovements.MoveBackward)) == 0)
            {
                this.RestoreOriginalStep();
            }
            return;
        }

        private void RestoreOriginalStep()
        {
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.Mission.NeedMovingBackward = false;
            this.Mission.RestoreConditions = true;
            if (this.Mission.FsmRestoreStateName == nameof(MissionMoveLoadElevatorState))
            {
                var newStep = new MissionMoveLoadElevatorState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else if (this.Mission.FsmRestoreStateName == nameof(MissionMoveCloseShutterState))
            {
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            this.Mission.FsmRestoreStateName = null;
        }

        #endregion
    }
}
