using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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
    public abstract class MissionMoveBase : IMissionMoveBase
    {
        #region Constructors

        protected MissionMoveBase(Mission mission,
             IServiceProvider serviceProvider,
             IEventAggregator eventAggregator)
        {
            this.Mission = mission;
            this.ServiceProvider = serviceProvider;
            this.EventAggregator = eventAggregator;

            this.BaysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.CellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.ElevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.ErrorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.LoadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();
            this.LoadingUnitsDataProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            this.MissionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.SensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.MachineModeDataProvider = this.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();

            this.Logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator { get; }

        public Mission Mission { get; set; }

        public IServiceProvider ServiceProvider { get; }

        internal IBaysDataProvider BaysDataProvider { get; }

        internal ICellsProvider CellsProvider { get; }

        internal IElevatorDataProvider ElevatorDataProvider { get; }

        internal IErrorsProvider ErrorsProvider { get; }

        internal ILoadingUnitMovementProvider LoadingUnitMovementProvider { get; }

        internal ILoadingUnitsDataProvider LoadingUnitsDataProvider { get; }

        internal ILogger<MachineManagerService> Logger { get; }

        internal IMachineModeVolatileDataProvider MachineModeDataProvider { get; }

        internal IMissionsDataProvider MissionsDataProvider { get; }

        internal ISensorsProvider SensorsProvider { get; }

        #endregion

        #region Methods

        public bool DepositUnitChangePosition()
        {
            bool bayShutter = false;
            using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
            {
                this.ElevatorDataProvider.SetLoadingUnit(null);

                if (this.Mission.LoadUnitDestination is LoadingUnitLocation.Cell)
                {
                    var destinationCellId = this.Mission.DestinationCellId;
                    if (destinationCellId.HasValue)
                    {
                        if (this.Mission.LoadUnitId > 0)
                        {
                            try
                            {
                                this.CellsProvider.SetLoadingUnit(destinationCellId.Value, this.Mission.LoadUnitId);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError($"SetLoadingUnit: Load Unit {this.Mission.LoadUnitId}; error {ex.Message}");
                                this.ErrorsProvider.RecordNew(MachineErrorCode.CellLogicallyOccupied, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.CellLogicallyOccupied, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                    }
                    else
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
                else
                {
                    var bayPosition = this.BaysDataProvider.GetPositionByLocation(this.Mission.LoadUnitDestination);
                    if (this.Mission.LoadUnitId > 0)
                    {
                        this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadUnitId);
                        this.LoadingUnitsDataProvider.SetHeight(this.Mission.LoadUnitId, 0);
                    }
                }

                transaction.Commit();
            }

            this.SendPositionNotification($"Load Unit {this.Mission.LoadUnitId} position changed");
            return bayShutter;
        }

        public void DepositUnitEnd(bool restore = false)
        {
            var bayShutter = false;
            if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                bayShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
            }
            if (restore)
            {
                this.DepositUnitChangePosition();
                this.Mission.RestoreState = MissionState.NotDefined;
                this.Mission.NeedMovingBackward = false;
            }

            this.MissionsDataProvider.Update(this.Mission);
            if (bayShutter)
            {
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                var newStep = new MissionMoveCloseShutterState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public void LoadUnitChangePosition()
        {
            using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
            {
                this.ElevatorDataProvider.SetLoadingUnit(this.Mission.LoadUnitId);

                if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell)
                {
                    var sourceCellId = this.Mission.LoadUnitCellSourceId;
                    if (sourceCellId.HasValue)
                    {
                        try
                        {
                            this.CellsProvider.SetLoadingUnit(sourceCellId.Value, null);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"SetLoadingUnit: Load Unit {this.Mission.LoadUnitId}; error {ex.Message}");
                            this.ErrorsProvider.RecordNew(MachineErrorCode.CellLogicallyOccupied, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.CellLogicallyOccupied, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                    }
                    else
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitSourceCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
                else
                {
                    var bayPosition = this.BaysDataProvider.GetPositionByLocation(this.Mission.LoadUnitSource);
                    this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                }

                transaction.Commit();
            }

            this.SendPositionNotification($"Load Unit {this.Mission.LoadUnitId} position changed");
        }

        public void LoadUnitEnd(bool restore = false)
        {
            if (restore)
            {
                this.LoadUnitChangePosition();
            }

            // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
            if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell
                && this.Mission.LoadUnitId > 0
                )
            {
                try
                {
                    this.Mission.DestinationCellId = this.CellsProvider.FindEmptyCell(this.Mission.LoadUnitId);
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.ErrorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull, this.Mission.TargetBay);
                    this.Mission.LoadUnitDestination = this.Mission.LoadUnitSource;
                    this.MissionsDataProvider.Update(this.Mission);
                    var newStep = new MissionMoveDepositUnitState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                    return;
                }
            }

            this.SendPositionNotification($"Load Unit {this.Mission.LoadUnitId} position changed");

            if (restore)
            {
                this.Mission.RestoreState = MissionState.NotDefined;
                this.Mission.NeedMovingBackward = false;
            }

            if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination == LoadingUnitLocation.Elevator
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

        public abstract void OnCommand(CommandMessage command);

        public abstract bool OnEnter(CommandMessage command);

        public abstract void OnNotification(NotificationMessage message);

        public virtual void OnResume(CommandMessage command)
        {
        }

        public virtual void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            if (this.Mission != null)
            {
                this.Mission.StopReason = reason;

                if (this.GetType().Name != nameof(MissionMoveErrorState)
                    && this.Mission.IsRestoringType()
                    )
                {
                    this.Mission.RestoreState = this.Mission.State;
                    if (moveBackward)
                    {
                        this.Mission.NeedMovingBackward = true;
                    }
                    var newStep = new MissionMoveErrorState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
        }

        public void SendMoveNotification(BayNumber targetBay, string description, bool isEject, MessageStatus messageStatus)
        {
            var messageData = new MoveLoadingUnitMessageData(
                this.Mission.MissionType,
                this.Mission.LoadUnitSource,
                this.Mission.LoadUnitDestination,
                this.Mission.LoadUnitCellSourceId,
                this.Mission.DestinationCellId,
                this.Mission.LoadUnitId,
                (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell),
                isEject,
                this.Mission.Id,
                this.Mission.Action,
                this.Mission.StopReason,
                this.Mission.State);

            var msg = new NotificationMessage(
                messageData,
                description,
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                MessageType.MoveLoadingUnit,
                this.Mission.TargetBay,
                targetBay,
                messageStatus);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        public void SendPositionNotification(string description)
        {
            var msg = new NotificationMessage(
                null,
                description,
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.Positioning,
                this.Mission.TargetBay,
                this.Mission.TargetBay,
                MessageStatus.OperationUpdateData);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        public bool UpdateResponseList(MessageType type)
        {
            bool update = false;
            switch (type)
            {
                case MessageType.Positioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Positioning;
                    update = true;
                    break;

                case MessageType.ShutterPositioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Shutter;
                    update = true;
                    break;

                case MessageType.Homing:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Homing;
                    update = true;
                    break;
            }
            return update;
        }

        #endregion
    }
}
