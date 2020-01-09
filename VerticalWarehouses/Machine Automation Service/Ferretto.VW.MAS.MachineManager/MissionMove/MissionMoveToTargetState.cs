using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveToTargetState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveToTargetState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitsDataProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
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
            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            this.Mission.EjectLoadUnit = false;
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveToTargetState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell)
            {
                var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitSource);
                if (bay is null)
                {
                    var description = $"{this.GetType().Name}: source bay not found {this.Mission.LoadingUnitSource}";

                    throw new StateMachineException(description,
                        new CommandMessage(null, null, MessageActor.Any, MessageActor.MachineManager, MessageType.MoveLoadingUnit, this.Mission.TargetBay, this.Mission.TargetBay),
                        MessageActor.MachineManager);
                }
                this.Mission.CloseShutterBayNumber = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
                measure = true;
            }

            var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            if (destinationHeight is null)
            {
                var description = $"GetSourceHeight error: position not found ({this.Mission.LoadingUnitSource} {(this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell ? this.Mission.LoadingUnitCellSourceId : this.Mission.LoadingUnitId)})";

                throw new StateMachineException(description,
                    new CommandMessage(null, null, MessageActor.Any, MessageActor.MachineManager, MessageType.MoveLoadingUnit, this.Mission.TargetBay, this.Mission.TargetBay),
                    MessageActor.MachineManager);
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                this.logger.LogDebug($"Homing elevator occupied start");
                this.loadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadingUnitId, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else
            {
                this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId);
            }
            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);
            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            var stopped = false;

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing)
                    {
                        // do not clear needHoming because will have to do it after unloading (DepositUnitState)
                        var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
                        this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
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
                                this.missionsDataProvider.Update(this.Mission);
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
                            this.missionsDataProvider.Update(this.Mission);
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
                        var check = this.loadingUnitsDataProvider.CheckWeight(this.Mission.LoadingUnitId);
                        if (check == MachineErrorCode.NoError)
                        {
                            this.baysDataProvider.Light(this.Mission.TargetBay, false);
                        }
                        else
                        {
                            // stop movement and  go back to bay
                            this.errorsProvider.RecordNew(check);
                            this.Mission.EjectLoadUnit = true;
                            this.Mission.LoadingUnitDestination = this.Mission.LoadingUnitSource;
                            this.missionsDataProvider.Update(this.Mission);
                            var newMessageData = new StopMessageData(StopRequestReason.Stop);
                            this.loadingUnitMovementProvider.StopOperation(newMessageData, notification.RequestingBay, MessageActor.MachineManager, notification.RequestingBay);
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
