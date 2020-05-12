﻿using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveDepositUnitStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveDepositUnitStep(Mission mission,
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

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.DepositUnit;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var bayNumber = this.Mission.TargetBay;
            switch (this.Mission.LoadUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId is null)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }

                    if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell
                        && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
                    {
                        var baySource = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                        if (baySource != null
                            && baySource.Shutter.Type != ShutterType.NotSpecified
                            )
                        {
                            var shutterInverter = baySource.Shutter.Inverter.Index;
                            if (this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed
                                && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half
                                )
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitShutterOpen, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                    }

                    var cell = this.CellsProvider.GetById(this.Mission.DestinationCellId.Value);

                    this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;

                    break;

                default:
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bay is null)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        bayNumber = bay.Number;
                        this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitDestination);
                        var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(bay.Number);
                        if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                        }
#if CHECK_BAY_SENSOR
                        if (bay.Carousel != null)
                        {
                            var result = this.LoadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadUnitDestination, deposit: true);
                            if (result != MachineErrorCode.NoError)
                            {
                                var error = this.ErrorsProvider.RecordNew(result, bayNumber);
                                throw new StateMachineException(error.Reason, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
#endif
                    }
                    break;
            }

            if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
            {
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.Logger.LogInformation($"Open Shutter Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, this.Mission.RestoreConditions);
                }
                else
                {
                    this.Logger.LogInformation($"Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition} Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, false, this.Mission.OpenShutterPosition, false, MessageActor.MachineManager, bayNumber, this.Mission.LoadUnitId, null);
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.ShutterPositioning
                        || notification.TargetBay == BayNumber.ElevatorBay
                        )
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                            if (notification.Type == MessageType.Positioning)
                            {
                                this.DepositUnitChangePosition();
                            }
                        }

                        if (notification.Type == MessageType.ShutterPositioning
                            && this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                            )
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                {
                                    this.Logger.LogInformation($"Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"ContinuePositioning Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.Logger.LogError(ErrorDescriptions.LoadUnitShutterClosed);
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                                break;
                            }
                        }
                        else if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                        {
                            this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal positioning end Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                        }

                        if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                            && (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                                || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                            )
                        {
                            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                            this.DepositUnitEnd();
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    {
                        this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                    }
                    break;
            }
        }

        #endregion
    }
}
