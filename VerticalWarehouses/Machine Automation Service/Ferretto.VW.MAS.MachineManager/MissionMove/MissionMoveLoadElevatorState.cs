using System;
using Ferretto.VW.CommonUtils.Messages;
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
    public class MissionMoveLoadElevatorState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveLoadElevatorState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();
            this.cellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.elevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveLoadElevatorState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
            switch (this.Mission.LoadingUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.Mission.LoadingUnitCellSourceId.Value);

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitSource);
                    if (bay is null)
                    {
                        var description = $"{this.GetType().Name}: source bay not found {this.Mission.LoadingUnitSource}";

                        throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    this.Mission.Direction = (bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);
                    this.Mission.OpenShutterPosition = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadingUnitSource);
                    if (this.Mission.OpenShutterPosition == this.sensorsProvider.GetShutterPosition(bay.Number))
                    {
                        this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    }
                    if (bay.Carousel != null)
                    {
                        var result = this.loadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadingUnitSource, deposit: false);
                        if (result != MachineErrorCode.NoError)
                        {
                            var error = this.errorsProvider.RecordNew(result);
                            throw new StateMachineException(error.Description, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                    }
                    break;
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                this.logger.LogDebug($"Homing elevator occupied start");
                this.loadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadingUnitId, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else if (this.Mission.NeedHomingAxis == Axis.BayChain)
            {
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.logger.LogDebug($"OpenShutter start");
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                }
                else
                {
                    this.logger.LogDebug($"MoveManualLoadingUnitForward start: direction {this.Mission.Direction}");
                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.logger.LogDebug($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}, measure {measure}");
                this.loadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, this.Mission.OpenShutterPosition, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadingUnitId);
            }
            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    var measure = (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell);
                    if (notification.Type == MessageType.Homing)
                    {
                        if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                        {
                            this.logger.LogDebug($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}, measure {measure}");
                            this.loadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, this.Mission.OpenShutterPosition, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadingUnitId);
                        }
                        else if (this.Mission.NeedHomingAxis == Axis.BayChain)
                        {
                            this.LoadUnitEnd();
                        }
                        this.Mission.NeedHomingAxis = Axis.None;
                        this.missionsDataProvider.Update(this.Mission);
                    }
                    else
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.missionsDataProvider.Update(this.Mission);
                        }

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                if (this.Mission.NeedHomingAxis == Axis.BayChain)
                                {
                                    this.logger.LogDebug($"MoveManualLoadingUnitForward start: direction {this.Mission.Direction}");
                                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                                }
                                else
                                {
                                    this.logger.LogDebug($"ContinuePositioning");
                                    this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                                this.OnStop(StopRequestReason.Error, !this.errorsProvider.IsErrorSmall());
                                break;
                            }
                        }
                        if ((this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified && (this.Mission.DeviceNotifications == (MissionDeviceNotifications.Positioning | MissionDeviceNotifications.Shutter)))
                            || (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified && (this.Mission.DeviceNotifications == MissionDeviceNotifications.Positioning))
                            )
                        {
                            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                            if (this.Mission.NeedHomingAxis == Axis.BayChain)
                            {
                                this.logger.LogDebug($"Homing Bay free start");
                                this.loadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadingUnitId, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.LoadUnitEnd();
                            }
                        }
                    }

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    {
                        this.OnStop(StopRequestReason.Error, !this.errorsProvider.IsErrorSmall());
                    }
                    break;
            }
        }

        private void LoadUnitEnd()
        {
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(this.Mission.LoadingUnitId);

                if (this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell)
                {
                    var sourceCellId = this.Mission.LoadingUnitCellSourceId;
                    if (sourceCellId.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(sourceCellId.Value, null);
                    }
                    else
                    {
                        throw new InvalidOperationException("");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.Mission.LoadingUnitSource);
                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                }

                transaction.Commit();
            }
            var msg = new NotificationMessage(
                            null,
                            $"Load Unit position changed",
                            MessageActor.Any,
                            MessageActor.MachineManager,
                            MessageType.Positioning,
                            this.Mission.TargetBay,
                            this.Mission.TargetBay,
                            MessageStatus.OperationUpdateData);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
            if (this.Mission.LoadingUnitSource != LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitId > 0
                )
            {
                try
                {
                    this.Mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.Mission.LoadingUnitId);
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.errorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull);
                    this.Mission.LoadingUnitDestination = this.Mission.LoadingUnitSource;
                    this.missionsDataProvider.Update(this.Mission);
                    var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                    return;
                }
            }

            this.missionsDataProvider.Update(this.Mission);
            if (this.Mission.LoadingUnitSource == LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination == LoadingUnitLocation.Elevator
                )
            {
                var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveToTargetState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
