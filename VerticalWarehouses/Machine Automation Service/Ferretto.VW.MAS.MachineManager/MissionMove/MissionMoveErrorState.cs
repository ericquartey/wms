using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveErrorState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveErrorState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        /// <summary>
        /// Puts the mission to sleep: note the use of ErrorMovements.
        /// All notifications will be ignored.
        /// Only a call to OnResume can wake up the mission.
        /// </summary>
        /// <param name="command">not used</param>
        public override bool OnEnter(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            this.Mission.State = MissionState.Error;
            this.MissionsDataProvider.Update(this.Mission);

            var newMessageData = new StopMessageData(StopRequestReason.Error);
            this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
            this.Mission.RestoreConditions = false;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.MissionsDataProvider.Update(this.Mission);

            bool isEject = this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation;
            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.StateName, isEject, MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            if (this.Mission.ErrorMovements != MissionErrorMovements.None)
            {
                var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning end");
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                if (this.Mission.RestoreState != MissionState.CloseShutter)
                                {
                                    if (this.Mission.NeedMovingBackward)
                                    {
                                        if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                                        {
                                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal back positioning start");
                                            this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                                        }
                                    }
                                    else
                                    {
                                        var isLoaded = (this.Mission.RestoreState == MissionState.DepositUnit);
                                        var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
                                        if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, isLoaded, measure, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                                        {
                                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal forward positioning start");
                                            this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                                        }
                                    }
                                }
                                if (!this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward) && !this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
                                {
                                    this.RestoreOriginalStep();
                                }
                                else
                                {
                                    this.MissionsDataProvider.Update(this.Mission);
                                }
                            }
                            else
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed);

                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveShutter;
                                this.MissionsDataProvider.Update(this.Mission);

                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                            }
                        }
                        else
                        {
                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal positioning end");
                            if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
                            {
                                this.Mission.NeedMovingBackward = false;
                                if (this.Mission.NeedHomingAxis == Axis.Horizontal
                                    && this.Mission.RestoreState == MissionState.DepositUnit
                                    )
                                {
                                    this.Mission.RestoreState = MissionState.NotDefined;
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
                                this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                                if (this.Mission.RestoreState == MissionState.LoadElevator)
                                {
                                    this.LoadUnitEnd(restore: true);
                                }
                                else
                                {
                                    this.DepositUnitEnd(restore: true);
                                }
                            }
                        }
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            this.Mission.ErrorMovements = MissionErrorMovements.None;
                            this.MissionsDataProvider.Update(this.Mission);

                            var newMessageData = new StopMessageData(StopRequestReason.Error);
                            this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                        }
                        break;
                }
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: Resume mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, from {this.Mission.RestoreStateName}, loadUnit {this.Mission.LoadUnitId}");

            switch (this.Mission.RestoreStateName)
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
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;

                case nameof(MissionMoveLoadElevatorState):
                    this.RestoreLoadElevatorStart();
                    break;

                case nameof(MissionMoveToTargetState):
                    this.Mission.RestoreConditions = true;
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.NeedMovingBackward = false;
                    this.Mission.StopReason = StopRequestReason.NoReason;
                    {
                        var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;

                case nameof(MissionMoveStartState):
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    this.Mission.StopReason = StopRequestReason.NoReason;
                    {
                        var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;

                case nameof(MissionMoveWaitPickState):
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    this.Mission.StopReason = StopRequestReason.NoReason;
                    {
                        var newStep = new MissionMoveWaitPickState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;

                default:
                    this.Logger.LogError($"{this.GetType().Name}: no valid FsmRestoreStateName {this.Mission.RestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    {
                        this.Mission.StopReason = StopRequestReason.NoReason;
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;
            }
        }

        private void RestoreBayChain()
        {
            this.Mission.StopReason = StopRequestReason.NoReason;
            if (this.LoadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.Mission.TargetBay))
            {
                // movement is finished
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                if (destination is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                this.Mission.LoadUnitDestination = destination.Location;

                var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
                {
                    this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId);
                    this.BaysDataProvider.SetLoadingUnit(origin.Id, null);
                    transaction.Commit();
                }

                var notificationText = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                this.SendMoveNotification(bay.Number, notificationText, false, MessageStatus.OperationWaitResume);

                this.Mission.RestoreState = MissionState.NotDefined;
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
                this.Mission.RestoreState = MissionState.NotDefined;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveBayChainState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreCloseShutter()
        {
            this.Mission.StopReason = StopRequestReason.NoReason;
            var shutterPosition = this.SensorsProvider.GetShutterPosition(this.Mission.TargetBay);
            if (shutterPosition != ShutterPosition.Opened)
            {
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.Logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutter;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.RestoreState = MissionState.NotDefined;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
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
            this.Mission.StopReason = StopRequestReason.NoReason;
            var destination = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            var current = this.LoadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!destination.HasValue || Math.Abs((destination.Value - current)) > 2))
            {
                if (this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.Logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.RestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            switch (this.Mission.LoadUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId != null)
                    {
                        var cell = this.CellsProvider.GetById(this.Mission.DestinationCellId.Value);
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
                    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                    // invert direction?
                    if (this.Mission.NeedMovingBackward)
                    {
                        this.Mission.Direction = (bay.Side != WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    else
                    {
                        this.Mission.Direction = (bay.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitDestination);
                    var shutterPosition = this.SensorsProvider.GetShutterPosition(this.Mission.TargetBay);
                    if (this.Mission.OpenShutterPosition == ShutterPosition.Half && shutterPosition == ShutterPosition.Opened)
                    {
                        this.Mission.OpenShutterPosition = shutterPosition;
                    }
                    if (shutterPosition != this.Mission.OpenShutterPosition)
                    {
                        this.Logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutter;
                        this.MissionsDataProvider.Update(this.Mission);
                        return;
                    }
                    break;
            }
            if (this.Mission.NeedMovingBackward
                || this.Mission.NeedHomingAxis == Axis.Horizontal
                )
            {
                this.Mission.NeedMovingBackward = true;
                this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal back positioning start");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
            }
            else
            {
                this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal forward positioning start");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if (!this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward) && !this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
            {
                this.RestoreOriginalStep();
            }
            else
            {
                this.MissionsDataProvider.Update(this.Mission);
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
            this.Mission.StopReason = StopRequestReason.NoReason;
            var origin = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            var current = this.LoadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!origin.HasValue || Math.Abs((origin.Value - current)) > 2))
            {
                if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.Logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.RestoreStateName} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.RestoreState = MissionState.NotDefined;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);

                    return;
                }
                else
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            switch (this.Mission.LoadUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.LoadUnitCellSourceId != null)
                    {
                        var cell = this.CellsProvider.GetById(this.Mission.LoadUnitCellSourceId.Value);

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
                    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                    // invert direction?
                    if (this.Mission.NeedMovingBackward)
                    {
                        this.Mission.Direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    else
                    {
                        this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                    var shutterPosition = this.SensorsProvider.GetShutterPosition(this.Mission.TargetBay);
                    if (this.Mission.OpenShutterPosition == ShutterPosition.Half && shutterPosition == ShutterPosition.Opened)
                    {
                        this.Mission.OpenShutterPosition = shutterPosition;
                    }
                    if (shutterPosition != this.Mission.OpenShutterPosition)
                    {
                        this.Logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning start");
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutter;
                        this.MissionsDataProvider.Update(this.Mission);
                        return;
                    }
                    break;
            }
            if (this.Mission.NeedMovingBackward)
            {
                this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal back positioning start");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitBack(this.Mission.Direction, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
            }
            else
            {
                this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal forward positioning start");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if (!this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward) && !this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
            {
                this.RestoreOriginalStep();
            }
            else
            {
                this.MissionsDataProvider.Update(this.Mission);
            }
        }

        private void RestoreOriginalStep()
        {
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.Mission.NeedMovingBackward = false;
            this.Mission.RestoreConditions = true;
            if (this.Mission.RestoreState == MissionState.LoadElevator)
            {
                var newStep = new MissionMoveLoadElevatorState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else if (this.Mission.RestoreState == MissionState.CloseShutter)
            {
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
