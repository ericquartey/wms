using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveToTargetState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveToTargetState(Mission mission,
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
            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            this.Mission.EjectLoadUnit = false;
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveToTargetState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitSource);
                if (bay is null)
                {
                    var description = $"{this.GetType().Name}: source bay not found {this.Mission.LoadingUnitSource}";

                    throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                this.Mission.CloseShutterBayNumber = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
                measure = true;
            }

            var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            if (destinationHeight is null)
            {
                var description = $"GetSourceHeight error: position not found ({this.Mission.LoadingUnitSource} {(this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadingUnitCellSourceId : this.Mission.LoadingUnitId)})";

                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                this.Logger.LogDebug($"Homing elevator occupied start");
                this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadingUnitId, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else
            {
                this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId);
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);
            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            var stopped = false;

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing)
                    {
                        // do not clear needHoming because will have to do it after unloading (DepositUnitState)
                        var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
                        this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                            this.Mission.CloseShutterBayNumber,
                            measure,
                            MessageActor.MachineManager,
                            notification.RequestingBay,
                            this.Mission.RestoreConditions,
                            targetBayPositionId,
                            targetCellId);
                    }
                    else
                    {
                        if (!this.Mission.EjectLoadUnit
                            || notification.Type == MessageType.ShutterPositioning
                            )
                        {
                            if (this.UpdateResponseList(notification.Type))
                            {
                                this.MissionsDataProvider.Update(this.Mission);
                            }
                        }
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    if (this.Mission.EjectLoadUnit)
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                        }
                    }
                    else
                    {
                        this.OnStop(StopRequestReason.Error);
                        stopped = true;
                    }
                    break;

                case MessageStatus.OperationUpdateData:
                    // check weight value
                    if (measure
                        && !this.Mission.EjectLoadUnit
                        && notification.Source != MessageActor.MachineManager
                        )
                    {
                        var check = this.LoadingUnitsDataProvider.CheckWeight(this.Mission.LoadingUnitId);
                        if (check == MachineErrorCode.NoError)
                        {
                            this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                        }
                        else
                        {
                            // stop movement and  go back to bay
                            this.ErrorsProvider.RecordNew(check);
                            this.Mission.EjectLoadUnit = true;
                            this.Mission.LoadingUnitDestination = this.Mission.LoadingUnitSource;
                            this.MissionsDataProvider.Update(this.Mission);
                            var newMessageData = new StopMessageData(StopRequestReason.Stop);
                            this.LoadingUnitMovementProvider.StopOperation(newMessageData, notification.RequestingBay, MessageActor.MachineManager, notification.RequestingBay);
                        }
                    }

                    break;
            }
            if (!stopped)
            {
                if ((this.Mission.CloseShutterBayNumber != BayNumber.None && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                    || (this.Mission.CloseShutterBayNumber == BayNumber.None && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                    )
                {
                    if (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Elevator
                        && !this.Mission.EjectLoadUnit
                        )
                    {
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    else
                    {
                        var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                }
            }
        }

        #endregion
    }
}
