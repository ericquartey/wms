using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels.Resources;
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
    public class MissionMoveDepositUnitState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveDepositUnitState(Mission mission,
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
            this.loadingUnitsDataProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();

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
            this.Mission.FsmStateName = nameof(MissionMoveDepositUnitState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var bayNumber = this.Mission.TargetBay;
            switch (this.Mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.Mission.DestinationCellId.Value);

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    if (bay is null)
                    {
                        var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                        throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    bayNumber = bay.Number;
                    this.Mission.OpenShutterPosition = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadingUnitDestination);
                    if (this.Mission.OpenShutterPosition == this.sensorsProvider.GetShutterPosition(bay.Number))
                    {
                        this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    }
                    if (bay.Carousel != null)
                    {
                        var result = this.loadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadingUnitDestination, deposit: true);
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
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.logger.LogDebug($"Open Shutter");
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, this.Mission.RestoreConditions);
                }
                else
                {
                    this.logger.LogDebug($"Manual Horizontal forward positioning start");
                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.logger.LogDebug($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}");
                this.loadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, false, this.Mission.OpenShutterPosition, false, MessageActor.MachineManager, bayNumber, null);
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
                    if (notification.Type == MessageType.Homing)
                    {
                        this.Mission.NeedHomingAxis = Axis.None;
                        this.DepositUnitEnd();
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
                                if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                                {
                                    this.logger.LogDebug($"Manual Horizontal forward positioning start");
                                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadingUnitId, MessageActor.MachineManager, this.Mission.TargetBay);
                                }
                                else
                                {
                                    this.logger.LogDebug($"ContinuePositioning");
                                    this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterClosed);
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
                            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                            {
                                this.logger.LogDebug($"Homing elevator free start");
                                this.loadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadingUnitId, notification.RequestingBay, MessageActor.MachineManager);
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
                        this.OnStop(StopRequestReason.Error, !this.errorsProvider.IsErrorSmall());
                    }
                    break;
            }
        }

        private void DepositUnitEnd()
        {
            bool bayShutter = false;
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(null);

                if (this.Mission.LoadingUnitDestination is LoadingUnitLocation.Cell)
                {
                    var destinationCellId = this.Mission.DestinationCellId;
                    if (destinationCellId.HasValue)
                    {
                        if (this.Mission.LoadingUnitId > 0)
                        {
                            this.cellsProvider.SetLoadingUnit(destinationCellId.Value, this.Mission.LoadingUnitId);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Loading unit movement to target cell has no target cell specified.");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.Mission.LoadingUnitDestination);
                    if (this.Mission.LoadingUnitId > 0)
                    {
                        this.baysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadingUnitId);
                        this.loadingUnitsDataProvider.SetHeight(this.Mission.LoadingUnitId, 0);
                    }
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                    bayShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
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

            this.missionsDataProvider.Update(this.Mission);
            if (bayShutter)
            {
                this.baysDataProvider.Light(this.Mission.TargetBay, true);
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
