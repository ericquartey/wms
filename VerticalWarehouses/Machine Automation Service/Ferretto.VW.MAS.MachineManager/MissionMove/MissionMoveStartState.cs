using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveStartState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveStartState(Mission mission,
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
            this.Mission.StateName = nameof(MissionMoveStartState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.LoadUnitSource is LoadingUnitLocation.Elevator)
            {
                var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
                if (destinationHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({this.Mission.LoadUnitSource} {(this.Mission.LoadUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadUnitCellSourceId : this.Mission.LoadUnitId)})";

                    throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                if (targetCellId != null)
                {
                    var bay = this.LoadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }

                this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure: false,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId);
            }
            else
            {
                var sourceHeight = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

                if (sourceHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({this.Mission.LoadUnitSource} {(this.Mission.LoadUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadUnitCellSourceId : this.Mission.LoadUnitId)})";

                    throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                }

                if (targetCellId != null)
                {
                    var bay = this.LoadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }

                this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure: false,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId);
            }
            this.Mission.Status = MissionStatus.Executing;
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            bool isEject = this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadingUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation;

            var notificationText = $"Load Unit {this.Mission.LoadUnitId} start movement to bay {this.Mission.LoadUnitDestination}";
            this.SendMoveNotification(this.Mission.TargetBay, notificationText, isEject, MessageStatus.OperationStart);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.UpdateResponseList(notification.Type))
                    {
                        this.MissionsDataProvider.Update(this.Mission);
                    }

                    if ((this.Mission.CloseShutterBayNumber != BayNumber.None && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                        || (this.Mission.CloseShutterBayNumber == BayNumber.None && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                        )
                    {
                        if (this.Mission.LoadUnitSource is LoadingUnitLocation.Elevator)
                        {
                            var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                        else
                        {
                            var newStep = new MissionMoveLoadElevatorState(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        #endregion
    }
}
