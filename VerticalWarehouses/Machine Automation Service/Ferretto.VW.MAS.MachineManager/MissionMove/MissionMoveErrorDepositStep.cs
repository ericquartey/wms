﻿using System;
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
    public class MissionMoveErrorDepositStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveErrorDepositStep(Mission mission,
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
            return this.EnterErrorState(MissionStep.ErrorDeposit);
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
                                if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
                                {
                                    // after opening the shutter i check again the situation
                                    this.RestoreDepositStart();
                                    break;
                                }
                                else
                                {
                                    this.Mission.ErrorMovements = MissionErrorMovements.None;
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
                                // after closing the shutter i check again the situation
                                this.RestoreDepositStart();
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
                            this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal positioning end Mission:Id={this.Mission.Id}");
                            if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
                            {
                                this.Mission.NeedMovingBackward = false;
                                var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                                var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

                                if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                                    && shutterInverter != InverterDriver.Contracts.InverterIndex.None
                                    && bay.Shutter != null
                                    && bay.Shutter.Type != ShutterType.NotSpecified
                                    && shutterPosition != this.Mission.CloseShutterPosition
                                    )
                                {
                                    this.Logger.LogInformation($"{this.GetType().Name}: Close Shutter positioning start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, restore: true, this.Mission.CloseShutterPosition);
                                    this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterClosed;
                                    this.MissionsDataProvider.Update(this.Mission);
                                }
                                else
                                {
                                    this.Mission.RestoreStep = MissionStep.NotDefined;
                                    this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveBackward;
                                    var newStep = new MissionMoveWaitChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                    newStep.OnEnter(null);
                                }
                            }
                            else
                            {
                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveForward;
                                this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                                this.DepositUnitEnd(restore: true);
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
                this.RestoreDepositStart();
            }
            else
            {
                this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
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
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.StopReason = StopRequestReason.NoReason;
            var destination = this.LoadingUnitMovementProvider.GetLastVerticalPosition();
            var current = this.LoadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((Math.Abs(destination - current) > 3)
                && this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell
                )
            {
                if (this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                {
                    this.Logger.LogDebug($"{this.GetType().Name}: Vertical position has changed {this.Mission.RestoreStep} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    this.Mission.RestoreConditions = true;
                    this.Mission.RestoreStep = MissionStep.NotDefined;
                    this.Mission.NeedMovingBackward = false;
                    var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
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
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            switch (this.Mission.LoadUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId != null)
                    {
                        this.Mission.ErrorMovements = MissionErrorMovements.None;
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
                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                    var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
                    var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                    if (shutterPosition != ShutterPosition.Opened
                        && shutterInverter != InverterDriver.Contracts.InverterIndex.None
                        && bay.Shutter != null
                        && bay.Shutter.Type != ShutterType.NotSpecified
                        && this.Mission.ErrorMovements == MissionErrorMovements.None
                        && this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)   // cannot move shutter if load unit is not in center
                        )
                    {
                        // in the first movement the shutter always goes to Opened
                        this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                        this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id} from position {shutterPosition} to {this.Mission.OpenShutterPosition}");
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, restore: true);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterOpen;
                        this.MissionsDataProvider.Update(this.Mission);
                        return;
                    }
                    else if (this.Mission.OpenShutterPosition == ShutterPosition.Half
                        && shutterInverter != InverterDriver.Contracts.InverterIndex.None
                        && bay.Shutter != null
                        && bay.Shutter.Type != ShutterType.NotSpecified
                        && shutterPosition != this.Mission.OpenShutterPosition
                        )
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id} from position {shutterPosition} to {this.Mission.OpenShutterPosition}");
                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, restore: true, this.Mission.OpenShutterPosition);
                        this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterOpen;
                        this.MissionsDataProvider.Update(this.Mission);
                        return;
                    }
                    this.Mission.ErrorMovements = MissionErrorMovements.None;
                    break;
            }
            if (this.Mission.NeedMovingBackward
                || this.Mission.NeedHomingAxis == Axis.Horizontal
                || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical
                )
            {
                this.Mission.NeedMovingBackward = true;
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
                if (this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay))
                {
                    this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                }
            }

            if (!this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward) && !this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveBackward))
            {
                if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
                {
                    this.RestoreOriginalStep();
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
            var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
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
