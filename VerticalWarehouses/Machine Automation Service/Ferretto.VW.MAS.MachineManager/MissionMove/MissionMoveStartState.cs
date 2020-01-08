using System;
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
    public class MissionMoveStartState : MissionMoveBase
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveStartState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
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
            this.Mission.FsmStateName = nameof(MissionMoveStartState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.LoadingUnitSource is LoadingUnitLocation.Elevator)
            {
                var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
                if (destinationHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({this.Mission.LoadingUnitSource} {(this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadingUnitCellSourceId : this.Mission.LoadingUnitId)})";

                    throw new StateMachineException(description);
                }
                if (targetCellId != null)
                {
                    var bay = this.loadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }

                this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
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
                var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

                if (sourceHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({this.Mission.LoadingUnitSource} {(this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadingUnitCellSourceId : this.Mission.LoadingUnitId)})";

                    throw new StateMachineException(description);
                }

                if (targetCellId != null)
                {
                    var bay = this.loadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }

                this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure: false,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId);
            }
            bool isEject = this.Mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;

            var newMessageData = new MoveLoadingUnitMessageData(
                this.Mission.MissionType,
                this.Mission.LoadingUnitSource,
                this.Mission.LoadingUnitDestination,
                this.Mission.LoadingUnitCellSourceId,
                this.Mission.DestinationCellId,
                this.Mission.LoadingUnitId,
                (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                isEject,
                this.Mission.FsmId,
                this.Mission.Action);

            var msg = new NotificationMessage(
                newMessageData,
                $"Loading Unit {this.Mission.LoadingUnitId} start movement to bay {this.Mission.LoadingUnitDestination}",
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                MessageType.MoveLoadingUnit,
                this.Mission.TargetBay,
                this.Mission.TargetBay,
                MessageStatus.OperationStart);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            this.Mission.Status = MissionStatus.Executing;
            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.UpdateResponseList(notification.Type))
                    {
                        this.missionsDataProvider.Update(this.Mission);
                    }

                    if ((this.Mission.CloseShutterBayNumber != BayNumber.None && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                        || (this.Mission.CloseShutterBayNumber == BayNumber.None && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                        )
                    {
                        if (this.Mission.LoadingUnitSource is LoadingUnitLocation.Elevator)
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
