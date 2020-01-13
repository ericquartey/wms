using System;
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
    public class MissionMoveDepositUnitState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveDepositUnitState(Mission mission,
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

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.RestoreStateName = null;
            this.Mission.StateName = nameof(MissionMoveDepositUnitState);
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
                    if (this.Mission.DestinationCellId != null)
                    {
                        var cell = this.CellsProvider.GetById(this.Mission.DestinationCellId.Value);

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }

                    break;

                default:
                    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                    if (bay is null)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    bayNumber = bay.Number;
                    this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitDestination);
                    if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(bay.Number))
                    {
                        this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    }
                    if (bay.Carousel != null)
                    {
                        var result = this.LoadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadUnitDestination, deposit: true);
                        if (result != MachineErrorCode.NoError)
                        {
                            var error = this.ErrorsProvider.RecordNew(result, bayNumber);
                            throw new StateMachineException(error.Description, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                    }
                    break;
            }

            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.Logger.LogDebug($"Open Shutter");
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, this.Mission.RestoreConditions);
                }
                else
                {
                    this.Logger.LogDebug($"Manual Horizontal forward positioning start");
                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.Logger.LogDebug($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}");
                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, false, this.Mission.OpenShutterPosition, false, MessageActor.MachineManager, bayNumber, null);
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing)
                    {
                        this.Mission.NeedHomingAxis = Axis.None;
                        this.DepositUnitEnd();
                    }
                    else
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                        }

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                                {
                                    this.Logger.LogDebug($"Manual Horizontal forward positioning start");
                                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"ContinuePositioning");
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
                        if ((this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                            || (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                            )
                        {
                            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                            {
                                this.Logger.LogDebug($"Homing elevator free start");
                                this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.DepositUnitEnd();
                            }
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
