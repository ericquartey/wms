using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
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

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveStartState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.cellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.elevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.machineModeDataProvider = this.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
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
            if (command != null
                && command.Data is IMoveLoadingUnitMessageData messageData
                )
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

                        throw new StateMachineException(description, command, MessageActor.MachineManager);
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
                        command.RequestingBay,
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

                        throw new StateMachineException(description, command, MessageActor.MachineManager);
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
                        command.RequestingBay,
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
                    messageData.CommandAction,
                    messageData.StopReason,
                    messageData.Verbosity);

                var msg = new NotificationMessage(
                    newMessageData,
                    $"Loading Unit {this.Mission.LoadingUnitId} start movement to bay {messageData.Destination}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.MoveLoadingUnit,
                    command.RequestingBay,
                    command.TargetBay,
                    MessageStatus.OperationStart);
                this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                this.Mission.Status = MissionStatus.Executing;
                this.Mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.Mission);
            }
            else
            {
                var description = $"Move Loading Unit Start State received wrong initialization data ({command.Data.GetType().Name})";

                throw new StateMachineException(description, command, MessageActor.MachineManager);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    this.UpdateResponseList(notification.Type);
                    this.missionsDataProvider.Update(this.Mission);

                    if ((this.Mission.CloseShutterBayNumber != BayNumber.None && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                        || (this.Mission.CloseShutterBayNumber == BayNumber.None && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                        )
                    {
                        if (this.Mission.LoadingUnitSource is LoadingUnitLocation.Elevator)
                        {
                            //returnValue = this.GetState<IMoveLoadingUnitDepositUnitState>();
                            var newStep = new MissionMoveNewState(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                        else
                        {
                            //returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
                            var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.OnStop(StopRequestReason.Error);
                    if (returnValue is IEndState endState)
                    {
                        endState.ErrorMessage = notification;
                    }
                    break;
            }
        }

        private void UpdateResponseList(MessageType type)
        {
            switch (type)
            {
                case MessageType.Positioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Positioning;
                    break;

                case MessageType.ShutterPositioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Shutter;
                    break;

                case MessageType.Homing:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Homing;
                    break;
            }
        }

        #endregion
    }
}
