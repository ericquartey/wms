﻿using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveErrorStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveErrorStep(Mission mission,
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
            return this.EnterErrorState(MissionStep.Error);
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
                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveShutterOpen;
                                if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveShutterClosed))
                                {
                                    this.Logger.LogInformation($"{this.GetType().Name}: Shutter Close start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false);
                                }
                                else
                                {
                                    this.Mission.ErrorMovements = MissionErrorMovements.None;
                                    this.RestoreOriginalStep();
                                }
                            }
                            else if (shutterPosition == ShutterPosition.Closed
                                && this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveShutterClosed)
                                )
                            {
                                this.Mission.ErrorMovements = MissionErrorMovements.None;
                                this.RestoreOriginalStep();
                            }
                            else
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed);

                                this.Mission.ErrorMovements &= ~MissionErrorMovements.MoveShutterClosed;
                                this.MissionsDataProvider.Update(this.Mission);

                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
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

            switch (this.Mission.RestoreStep)
            {
                case MissionStep.BayChain:
                    if (this.Mission.ErrorMovements == MissionErrorMovements.None)
                    {
                        this.RestoreBayChain();
                    }
                    else
                    {
                        this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
                    }
                    break;

                case MissionStep.CloseShutter:
                    if (this.Mission.ErrorMovements == MissionErrorMovements.None)
                    {
                        this.RestoreCloseShutter();
                    }
                    else
                    {
                        this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
                    }
                    break;

                case MissionStep.End:
                    this.Mission.RestoreConditions = false;
                    this.Mission.NeedMovingBackward = false;
                    {
                        var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;

                case MissionStep.ToTarget:
                case MissionStep.BackToTarget:
                    if (this.Mission.ErrorMovements == MissionErrorMovements.None)
                    {
                        this.RestoreMoveToTarget();
                    }
                    else
                    {
                        this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
                    }
                    break;

                case MissionStep.Start:
                    if (this.Mission.ErrorMovements == MissionErrorMovements.None)
                    {
                        this.RestoreStartStep();
                    }
                    else
                    {
                        this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
                    }
                    break;

                case MissionStep.WaitPick:
                    if (this.Mission.ErrorMovements == MissionErrorMovements.None)
                    {
                        this.RestoreWaitPick();
                    }
                    else
                    {
                        this.Logger.LogWarning($"{this.GetType().Name}: Resume mission {this.Mission.Id} already executed!");
                    }
                    break;

                default:
                    this.Logger.LogError($"{this.GetType().Name}: no valid RestoreState {this.Mission.RestoreStep} for mission {this.Mission.Id}, wmsId {this.Mission.WmsId}, loadUnit {this.Mission.LoadUnitId}");

                    {
                        this.Mission.StopReason = StopRequestReason.NoReason;
                        var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    break;
            }
        }

        private void RestoreBayChain()
        {
            this.Mission.StopReason = StopRequestReason.NoReason;
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
            if (destination is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.Opened
                && shutterPosition != ShutterPosition.Closed)
            {
                this.Mission.RestoreConditions = true;
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterOpen;
                this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterClosed;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else if ((destination.LoadingUnit is null || destination.LoadingUnit.Id == this.Mission.LoadUnitId)
                && this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                && this.LoadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.Mission.TargetBay)
                )
            {
                // movement is finished
                this.Mission.LoadUnitDestination = destination.Location;

                var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
                {
                    this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId);
                    this.BaysDataProvider.SetLoadingUnit(origin.Id, null);
                    transaction.Commit();
                }

                var notificationText = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                this.SendMoveNotification(bay.Number, notificationText, MessageStatus.OperationWaitResume);

                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.RestoreConditions = false;
                this.Mission.NeedMovingBackward = false;
                if (this.Mission.MissionType == MissionType.OUT
                    || this.Mission.MissionType == MissionType.WMS
                    )
                {
                    var newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreCloseShutter()
        {
            this.Mission.StopReason = StopRequestReason.NoReason;
            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.Opened)
            {
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterOpen;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.NeedMovingBackward = false;
                var newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreMoveToTarget()
        {
            this.Mission.RestoreConditions = true;
            this.Mission.NeedMovingBackward = false;
            this.Mission.StopReason = StopRequestReason.NoReason;
            if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
                var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                if (shutterPosition != ShutterPosition.Opened
                    && shutterPosition != ShutterPosition.Closed
                    )
                {
                    this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                    this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                    this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterOpen;
                    this.MissionsDataProvider.Update(this.Mission);
                    return;
                }
            }

            this.Mission.RestoreStep = MissionStep.NotDefined;
            var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        private void RestoreOriginalStep()
        {
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.Mission.NeedMovingBackward = false;
            this.Mission.RestoreConditions = true;
            IMissionMoveBase newStep;
            switch (this.Mission.RestoreStep)
            {
                case MissionStep.BayChain:
                    this.RestoreBayChain();
                    return;

                case MissionStep.CloseShutter:
                    newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    break;

                case MissionStep.Start:
                    newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    break;

                case MissionStep.WaitPick:
                    newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    break;

                default:
                    newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    break;
            }
            newStep.OnEnter(null);
        }

        private void RestoreStartStep()
        {
            this.Mission.NeedMovingBackward = false;
            this.Mission.StopReason = StopRequestReason.NoReason;
            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.Opened
                && shutterPosition != ShutterPosition.Closed)
            {
                this.Mission.RestoreConditions = true;
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterOpen;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else
            {
                this.Mission.RestoreConditions = true;
                this.Mission.RestoreStep = MissionStep.NotDefined;
                var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private void RestoreWaitPick()
        {
            this.Mission.NeedMovingBackward = false;
            this.Mission.StopReason = StopRequestReason.NoReason;
            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(this.Mission.TargetBay);
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.Opened
                && shutterPosition != ShutterPosition.Closed)
            {
                this.Mission.RestoreConditions = true;
                this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                this.Logger.LogInformation($"{this.GetType().Name}: Manual Shutter positioning start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, true);
                this.Mission.ErrorMovements = MissionErrorMovements.MoveShutterOpen;
                this.Mission.ErrorMovements |= MissionErrorMovements.MoveShutterClosed;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else
            {
                this.Mission.RestoreConditions = false;
                var newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
