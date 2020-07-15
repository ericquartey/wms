﻿using System;
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
    public class MissionMoveErrorLoadStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveErrorLoadStep(Mission mission,
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
        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            return this.EnterErrorState(MissionStep.ErrorLoad);
        }

        public override void OnNotification(NotificationMessage notification)
        {
            if (this.Mission.ErrorMovements != MissionErrorMovements.None
                || notification.Type == MessageType.Homing
                )
            {
                var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        if (notification.Type == MessageType.Homing
                            && notification.Data is HomingMessageData messageData
                            )
                        {
                            this.OnHomingNotification(messageData);
                        }
                        else if (notification.Type == MessageType.ShutterPositioning)
                        {
                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Shutter positioning end Mission:Id={this.Mission.Id}");
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition == this.Mission.OpenShutterPosition
                                && this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveShutterOpen)
                                )
                            {
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell)
                                {
                                    this.CloseShutter();
                                    break;
                                }
                                else
                                {
                                    this.ShutterPositionEnd();
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
                            else if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveShutterClosed))
                            {
                                // after closing the shutter i can move back to the previous step
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                this.Mission.RestoreConditions = true;
                                this.Mission.NeedMovingBackward = false;
                                this.Mission.RestoreStep = MissionStep.NotDefined;
                                var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
                            else
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed);

                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveShutterOpen;
                                this.MissionsDataProvider.Update(this.Mission);

                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                            }
                        }
                        else
                        {
                            this.Logger.LogDebug($"{this.GetType().Name}: Manual Horizontal positioning end Mission:Id={this.Mission.Id}");

                            // Perform the operation if machine is regular or machine is 1Ton machine and notification type is MessageType.CombinedMovements
                            if (!this.MachineVolatileDataProvider.IsOneTonMachine.Value ||
                                (this.MachineVolatileDataProvider.IsOneTonMachine.Value && notification.Type == MessageType.CombinedMovements))
                            {
                                if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
                                {
                                    this.Mission.NeedMovingBackward = false;
                                    if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                                       || this.Mission.CloseShutterPosition == ShutterPosition.NotSpecified
                                    )
                                    {
                                        this.RestoreOriginalStep();
                                    }
                                    else
                                    {
                                        this.CloseShutter();
                                    }
                                }
                                else
                                {
                                    this.Mission.ErrorMovements = MissionErrorMovements.None;
                                    this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                                    this.LoadUnitEnd(restore: true);
                                }
                            }
                        }
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            if (notification.Type != MessageType.Homing)
                            {
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                this.MissionsDataProvider.Update(this.Mission);

                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                            }
                        }
                        break;
                }
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: Resume mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, from {this.Mission.RestoreStep}, loadUnit {this.Mission.LoadUnitId}");

            if (this.Mission.ErrorMovements == MissionErrorMovements.None)
            {
                this.RestoreLoadElevatorStart();
            }
            else
            {
                this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
            }
        }

        private void CloseShutter()
        {
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
            this.Logger.LogInformation($"{this.GetType().Name}: Close Shutter positioning start Mission:Id={this.Mission.Id}");
            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, restore: true, this.Mission.CloseShutterPosition);
            this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterClosed;
            this.MissionsDataProvider.Update(this.Mission);
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
            if (this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
            {
                var loadUnitOnBoard = this.ElevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnitOnBoard is null
                    && this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                    )
                {
                    this.LoadUnitChangePosition();
                    loadUnitOnBoard = this.ElevatorDataProvider.GetLoadingUnitOnBoard();
                }
                if (loadUnitOnBoard != null
                    && loadUnitOnBoard.Id == this.Mission.LoadUnitId
                    && loadUnitOnBoard.Height > 0
                    )
                {
                    this.Logger.LogDebug($"{this.GetType().Name}: Load unit detected on board for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");
                    this.Mission.RestoreStep = MissionStep.ToTarget;
                    this.Mission.StepTime = DateTime.UtcNow;
                    var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnResume(null);

                    return;
                }
            }
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.StopReason = StopRequestReason.NoReason;
            var origin = this.LoadingUnitMovementProvider.GetLastVerticalPosition();
            var current = this.LoadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((Math.Abs(origin - current) > 3)
                && this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                )
            {
                if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.Logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.RestoreStep} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.RestoreStep = MissionStep.NotDefined;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
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
            int? positionId = null;
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
                    var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitSource);
                    positionId = bayPosition.Id;

                    // TODO: extend this sensor check also to external bay
                    if ((this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator) || this.SensorsProvider.IsDrawerPartiallyOnCradle)
                        && this.SensorsProvider.IsLoadingUnitInLocation(bayPosition.Location)
                        )
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                    }

                    //// always invert direction and do homing when loading from bay
                    //this.Mission.NeedMovingBackward = true;
                    //this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                    if (this.Mission.NeedMovingBackward)
                    {
                        this.Mission.Direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    else
                    {
                        this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    if (bay.Carousel != null
                        && !bayPosition.IsUpper
                        )
                    {
                        // in lower carousel position there is no profile check barrier
                        measure = false;
                    }
                    this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitSource);
                    var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
                    var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                    if (this.Mission.OpenShutterPosition == ShutterPosition.Half && shutterPosition == ShutterPosition.Opened)
                    {
                        this.Mission.OpenShutterPosition = shutterPosition;
                    }
                    if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                        && shutterPosition != this.Mission.OpenShutterPosition
                        )
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterOpen;
                        this.MissionsDataProvider.Update(this.Mission);
                        return;
                    }
                    break;
            }
            if (this.Mission.NeedMovingBackward)
            {
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal back positioning start Mission:Id={this.Mission.Id}");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitBackward(this.Mission.Direction, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
                else
                {
                    // no need to move back! restore original direction
                    this.Mission.NeedMovingBackward = false;
                    this.Mission.Direction = (this.Mission.Direction == HorizontalMovementDirection.Forwards ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);
                }
            }
            else
            {
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadUnitId, positionId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if (!this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward)
                && !this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward)
                )
            {
                if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell
                    && this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified
                    )
                {
                    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                    this.Logger.LogInformation($"{this.GetType().Name}: Close Shutter positioning start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, restore: true, this.Mission.CloseShutterPosition);
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterClosed;
                    this.MissionsDataProvider.Update(this.Mission);
                }
                else
                {
                    this.RestoreOriginalStep();
                }
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
            var newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        private void ShutterPositionEnd()
        {
            if (this.Mission.NeedMovingBackward)
            {
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitBackward(this.Mission.Direction, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal back positioning start Mission:Id={this.Mission.Id}");
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveBackward;
                }
            }
            else
            {
                var isLoaded = (this.Mission.RestoreStep == MissionStep.DepositUnit);
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, isLoaded, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }
        }

        #endregion
    }
}
