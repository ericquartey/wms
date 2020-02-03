﻿using System;
using System.Linq;
using Ferretto.VW.CommonUtils;
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
            this.MachineVolatileDataProvider = this.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            this.MissionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.SensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();

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

        public IMachineVolatileDataProvider MachineVolatileDataProvider { get; }

        internal ILogger<MachineManagerService> Logger { get; }

        internal IMissionsDataProvider MissionsDataProvider { get; }

        internal ISensorsProvider SensorsProvider { get; }

        #endregion

        #region Methods

        public bool CheckBayHeight(Bay locationBay, LoadingUnitLocation bayLocation, Mission mission)
        {
            bool returnValue = false;
#if CHECK_PROFILE
            var unitToMove = this.LoadingUnitsDataProvider.GetById(mission.LoadUnitId);
            var bayPosition = locationBay.Positions.First(w => w.Location == bayLocation);
            var bay = this.BaysDataProvider.GetByNumber(locationBay.Number);
            if (unitToMove != null
                && bay != null
                )
            {
                if (unitToMove.Height <= bayPosition.MaxSingleHeight)
                {
                    returnValue = true;
                }
                else if (bayPosition.MaxDoubleHeight > 0
                    && unitToMove.Height <= bayPosition.MaxDoubleHeight)
                {
                    if (bay.Positions.Count() == 1)
                    {
                        returnValue = true;
                    }
                    else if (!bayPosition.IsUpper
                        && bay.Positions.Any(p => p.IsUpper)
                        && bay.Positions.First(p => p.IsUpper).LoadingUnit == null)
                    {
                        returnValue = true;
                    }
                }
            }
#else
            returnValue = true;
#endif
            return returnValue;
        }

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
                    // we set LoadUnit height to zero, but not in lower carousel position, because there is not a profile check barrier
                    if (bayPosition.Bay.Carousel != null
                        && !bayPosition.IsUpper)
                    {
                        this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadUnitId);
                    }
                    else
                    {
                        this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadUnitId, 0);
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
                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.NeedMovingBackward = false;
            }

            if (bayShutter)
            {
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                var newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public bool EnterErrorState(MissionStep errorState)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            this.Mission.Step = errorState;
            this.MissionsDataProvider.Update(this.Mission);

            var newMessageData = new StopMessageData(StopRequestReason.Error);
            this.LoadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
            this.Mission.RestoreConditions = false;
            this.Mission.ErrorMovements = MissionErrorMovements.None;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);

            return true;
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
                // if we load from bay and load unit height is not compliant with the bay we go back
                var sourceBay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (sourceBay != null
                    && !this.CheckBayHeight(sourceBay, this.Mission.LoadUnitSource, this.Mission)
                    )
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitHeightExceeded, this.Mission.TargetBay);
                    this.MoveBackToBay();
                    return;
                }
                try
                {
                    this.Mission.DestinationCellId = this.CellsProvider.FindEmptyCell(this.Mission.LoadUnitId);
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.ErrorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull, this.Mission.TargetBay);
                    this.MoveBackToBay();
                    return;
                }
            }

            this.SendPositionNotification($"Load Unit {this.Mission.LoadUnitId} position changed");

            if (restore)
            {
                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.NeedMovingBackward = false;
            }

            if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination == LoadingUnitLocation.Elevator
                )
            {
                var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public abstract void OnCommand(CommandMessage command);

        public abstract bool OnEnter(CommandMessage command, bool showErrors = true);

        public void OnHomingNotification(HomingMessageData messageData)
        {
            if ((messageData.AxisToCalibrate == Axis.Horizontal || messageData.AxisToCalibrate == Axis.HorizontalAndVertical)
                && this.Mission.NeedHomingAxis == Axis.Horizontal
                && !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)
                )
            {
                this.Mission.NeedHomingAxis = Axis.None;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else if (messageData.AxisToCalibrate == Axis.BayChain
                    && this.Mission.NeedHomingAxis == Axis.BayChain
                )
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (bay != null
                    && bay.Positions != null
                    && bay.Positions.All(p => p.LoadingUnit is null)
                    )
                {
                    this.Mission.NeedHomingAxis = Axis.None;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = true;
                }
            }
        }

        public abstract void OnNotification(NotificationMessage message);

        public virtual void OnResume(CommandMessage command)
        {
        }

        public virtual void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            if (this.Mission != null)
            {
                this.Mission.StopReason = reason;

                IMissionMoveBase newStep;
                if (this.Mission.Step >= MissionStep.Error
                    && !this.Mission.IsRestoringType()
                    )
                {
                    newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else if (this.Mission.Step < MissionStep.Error)
                {
                    if (this.Mission.RestoreStep == MissionStep.NotDefined)
                    {
                        this.Mission.RestoreStep = this.Mission.Step;
                    }
                    if (moveBackward)
                    {
                        this.Mission.NeedMovingBackward = true;
                    }
                    if (this.Mission.Step == MissionStep.LoadElevator)
                    {
                        newStep = new MissionMoveErrorLoadStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else if (this.Mission.Step == MissionStep.DepositUnit)
                    {
                        newStep = new MissionMoveErrorDepositStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    newStep.OnEnter(null);
                }
                else
                {
                    this.OnEnter(null);
                }

                var stopMachineData = new ChangeRunningStateMessageData(false, null, CommandAction.Start, StopRequestReason.Stop);
                var stopMachineMessage = new CommandMessage(stopMachineData,
                    "Positioning OperationError",
                    MessageActor.MachineManager,
                    MessageActor.DeviceManager,
                    MessageType.ChangeRunningState,
                    this.Mission.TargetBay);
                this.EventAggregator.GetEvent<CommandEvent>().Publish(stopMachineMessage);
            }
        }

        public void SendMoveNotification(BayNumber targetBay, string description, MessageStatus messageStatus)
        {
            var messageData = new MoveLoadingUnitMessageData(
                this.Mission.MissionType,
                this.Mission.LoadUnitSource,
                this.Mission.LoadUnitDestination,
                this.Mission.LoadUnitCellSourceId,
                this.Mission.DestinationCellId,
                this.Mission.LoadUnitId,
                (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell),
                this.Mission.Id,
                this.Mission.Action,
                this.Mission.StopReason,
                this.Mission.Step);

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

        private void MoveBackToBay()
        {
            this.Mission.LoadUnitDestination = this.Mission.LoadUnitSource;
            this.MissionsDataProvider.Update(this.Mission);
            var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        #endregion
    }
}
