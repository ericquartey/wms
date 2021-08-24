using System;
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
            this.MachineProvider = this.ServiceProvider.GetRequiredService<IMachineProvider>();

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

        public IMachineProvider MachineProvider { get; }

        #endregion

        #region Methods

        /// <summary>
        /// return true if load unit height is ok for the bay location
        /// </summary>
        /// <param name="locationBay"></param>
        /// <param name="bayLocation"></param>
        /// <param name="mission"></param>
        /// <returns></returns>
        public bool CheckBayHeight(Bay locationBay, LoadingUnitLocation bayLocation, Mission mission, out bool canRetry, out MachineErrorCode errorCode)
        {
            var returnValue = false;
            canRetry = false;
            errorCode = MachineErrorCode.NoError;
#if CHECK_PROFILE
            var unitToMove = this.LoadingUnitsDataProvider.GetById(mission.LoadUnitId);
            if (unitToMove != null
                && locationBay != null
                )
            {
                var bayPosition = locationBay.Positions.First(w => w.Location == bayLocation);
                const double tolerance = 2.5;       // TODO use a parameter for this value??

                var machine = this.MachineProvider.GetMinMaxHeight();
                if (unitToMove.Height > machine.LoadUnitMaxHeight + tolerance)
                {
                    this.Logger.LogWarning($"Load unit Height {unitToMove.Height:0.00} higher than machine max {machine.LoadUnitMaxHeight}: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                    errorCode = MachineErrorCode.LoadUnitHeightFromBayExceeded;
                }
                else if (unitToMove.Height < machine.LoadUnitMinHeight - tolerance)
                {
                    this.Logger.LogWarning($"Load unit Height {unitToMove.Height:0.00} lower than machine min {machine.LoadUnitMinHeight}: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                    errorCode = MachineErrorCode.LoadUnitHeightFromBayTooLow;
                }
                else if (unitToMove.Height < bayPosition.MaxSingleHeight + tolerance)
                {
                    returnValue = true;
                }
                else if (bayPosition.MaxDoubleHeight > 0
                    && unitToMove.Height < bayPosition.MaxDoubleHeight + tolerance
                    )
                {
                    if (locationBay.Positions.Count() == 1)
                    {
                        returnValue = true;
                    }
                    else if (!bayPosition.IsUpper
                        && locationBay.Positions.Any(p => p.IsUpper)
                        && locationBay.Positions.First(p => p.IsUpper).LoadingUnit == null)
                    {
                        returnValue = true;
                    }
                    else
                    {
                        this.Logger.LogWarning($"Load unit Height {unitToMove.Height:0.00} higher than single {bayPosition.MaxSingleHeight} and upper position occupied: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                        canRetry = true;
                        errorCode = MachineErrorCode.LoadUnitHeightFromBayExceeded;
                    }
                }
                else if (bayPosition.MaxDoubleHeight == 0
                    && unitToMove.Height > bayPosition.MaxSingleHeight + tolerance)
                {
                    this.Logger.LogWarning($"Load unit Height {unitToMove.Height:0.00} higher than single {bayPosition.MaxSingleHeight}: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                    errorCode = MachineErrorCode.LoadUnitHeightFromBayExceeded;
                }
                else
                {
                    this.Logger.LogWarning($"Load unit Height {unitToMove.Height:0.00} higher than double {bayPosition.MaxDoubleHeight}: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                    errorCode = MachineErrorCode.LoadUnitHeightFromBayExceeded;
                }
                if (returnValue
                    && mission.MissionType == MissionType.FirstTest
                    && unitToMove.Height > machine.LoadUnitMinHeight + (3 * tolerance))
                {
                    returnValue = false;
                    this.Logger.LogWarning($"First test Load unit Height {unitToMove.Height:0.00} higher than machine min {machine.LoadUnitMinHeight}: Mission:Id={mission.Id}, Load Unit {mission.LoadUnitId} ");
                    errorCode = MachineErrorCode.LoadUnitHeightFromBayExceeded;
                }
            }
#else
            returnValue = true;
#endif
            return returnValue;
        }

        public bool DepositUnitChangePosition()
        {
            var bayShutter = false;
            using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
            {
                this.ElevatorDataProvider.SetLoadingUnit(null);

                if (this.Mission.LoadUnitDestination is LoadingUnitLocation.Cell)
                {
                    var destinationCellId = this.Mission.DestinationCellId;
                    if (destinationCellId.HasValue)
                    {
                        var lu = this.LoadingUnitsDataProvider.GetById(this.Mission.LoadUnitId);
                        if (this.Mission.LoadUnitId > 0
                            && (lu.CellId == null || lu.CellId != destinationCellId))
                        {
                            try
                            {
                                this.CellsProvider.SetLoadingUnit(destinationCellId.Value, this.Mission.LoadUnitId);
                                this.Logger.LogDebug($"SetLoadingUnit: Load Unit {this.Mission.LoadUnitId}; Cell id {destinationCellId}");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                this.Logger.LogError($"SetLoadingUnit: Load Unit {this.Mission.LoadUnitId}; error {ex.Message}");
                                this.ErrorsProvider.RecordNew(MachineErrorCode.CellLogicallyOccupied, this.Mission.TargetBay, ex.Message);
                                throw new StateMachineException(ErrorDescriptions.CellLogicallyOccupied, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
                else if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator)
                {
                    var bayPosition = this.BaysDataProvider.GetPositionByLocation(this.Mission.LoadUnitDestination);
                    if (!bayPosition.Bay.IsExternal
                        && !this.SensorsProvider.IsLoadingUnitInLocation(bayPosition.Location)
                        )
                    {
                        transaction.Rollback();
                        var error = bayPosition.IsUpper ? MachineErrorCode.TopLevelBayEmpty : MachineErrorCode.BottomLevelBayEmpty;
                        var description = bayPosition.IsUpper ? ErrorDescriptions.TopLevelBayEmpty : ErrorDescriptions.BottomLevelBayEmpty;
                        this.ErrorsProvider.RecordNew(error, this.Mission.TargetBay);
                        throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, this.Mission.LoadUnitId);
                }

                transaction.Commit();
            }

            this.SendPositionNotification($"Load Unit {this.Mission.LoadUnitId} position changed");
            return bayShutter;
        }

        private bool isWaitingMissionOnThisBay(Bay bay)
        {
            var retValue = false;

            if (bay != null)
            {
                if (bay.IsDouble)
                {
                    // List of waiting mission on the bay
                    var waitMissions = this.MissionsDataProvider.GetAllMissions()
                        .Where(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            (m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick)
                        );

                    retValue = waitMissions.Any();
                }
            }

            return retValue;
        }

        public void DepositUnitEnd(bool restore = false)
        {
            var bayShutter = false;
            Bay bay = null;
            if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
            {
                bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

                // ------------------------------
                // Add this block code
                if (!(bay.IsDouble &&
                        bay.Carousel == null &&
                        !bay.IsExternal
                     )
                )
                {
                    // all bays different from BID
                    bayShutter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified && !bay.IsExternal);
                }
                else
                {
                    // BID
                    bayShutter = bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified && !this.isWaitingMissionOnThisBay();
                }

                if (bayShutter)
                {
                    var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(bay.Number);
                    var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                    var shutterClosed = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                    if (shutterPosition == shutterClosed)
                    {
                        bayShutter = false;
                    }
                    else
                    {
                        this.Mission.CloseShutterPosition = shutterClosed;
                    }
                }
                // ---------------------

                //bayShutter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified);
                //if (bayShutter)
                //{
                //    var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(bay.Number);
                //    var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                //    var shutterClosed = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                //    if (shutterPosition == shutterClosed)
                //    {
                //        bayShutter = false;
                //    }
                //    else
                //    {
                //        this.Mission.CloseShutterPosition = shutterClosed;
                //    }
                //}
            }
            if (restore)
            {
                this.DepositUnitChangePosition();
                this.Mission.RestoreStep = MissionStep.NotDefined;
                this.Mission.NeedMovingBackward = false;
            }

            IMissionMoveBase newStep;
            if (bayShutter)
            {
                newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else
            {
                if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell)
                {
                    //if (this.MachineVolatileDataProvider.IsBayLightOn.ContainsKey(this.Mission.TargetBay)
                    //    && this.MachineVolatileDataProvider.IsBayLightOn[this.Mission.TargetBay]
                    //    && (this.MissionsDataProvider.GetAllActiveMissions().Count(m => m.Status != MissionStatus.New) <= 1)
                    //    )
                    //{
                    //    this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                    //}
                    newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                }
                else
                {
                    if (bay == null
                        || bay.Positions.Count() == 1
                        || bay.Positions.Any(x => x.Location == this.Mission.LoadUnitDestination && x.IsUpper)
                        || bay.Positions.Any(x => x.IsUpper && x.IsBlocked)
                        || bay.Carousel is null)
                    {
                        if (bay?.External != null &&
                            bay?.Positions.Count() == 2)
                        {
                            // double External bay movement
                            if ((this.isWaitingMissionOnThisBay(bay) || this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)) && this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
                            {
                                //newStep = new MissionMoveWaitDepositExternalBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep = new MissionMoveElevatorBayUpStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                            else
                            {
                                newStep = new MissionMoveDoubleExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                        }
                        else if (bay?.External != null)
                        {
                            // External bay movement
                            newStep = new MissionMoveExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            // Use a flag to prompt showing the MachineErrorCode.LoadUnitWeightExceeded condition
                            var bShowErrorCondition = true;
                            if (this.Mission.MissionType == MissionType.IN && this.Mission.ErrorCode != MachineErrorCode.NoError)
                            {
                                if (this.Mission.ErrorCode != MachineErrorCode.NoError)
                                {
                                    newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                }
                                else
                                {
                                    newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                }

                                // In this case (see conditions), close the shutter
                                //newStep = new MissionMoveCloseShutterStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                            // For the mission of OUT type or WMS type
                            else if (this.Mission.MissionType == MissionType.OUT ||
                                this.Mission.MissionType == MissionType.WMS ||
                                this.Mission.MissionType == MissionType.FullTestOUT)
                            {
                                // Go to the waiting step
                                newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                            else
                            {
                                // Detect the error condition and add it to the errors list
                                if (!this.CheckMissionShowError())
                                {
                                    // Set the light bay to ON
                                    this.BaysDataProvider.Light(this.Mission.TargetBay, true);

                                    if (this.Mission.MissionType != MissionType.Manual || this.Mission.MissionType != MissionType.ScaleCalibration)
                                    {
                                        this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, true);
                                    }
                                }
                                // End the mission
                                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                        }
                    }
                    else
                    {
                        // Handle the Carousel bay movement
                        if (this.Mission.MissionType == MissionType.Manual || this.Mission.MissionType == MissionType.ScaleCalibration)
                        {
                            newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            //newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep = new MissionMoveElevatorBayUpStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                    }
                }
            }
            newStep.OnEnter(null);
        }

        /// <summary>
        /// Check if exist at least a waiting mission (step == MissionStep.WaitPick) in the current bay.
        /// Applied only for double bay.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if exists at least a waiting mission,
        ///     <c>false</c> otherwise.
        /// </returns>
        private bool isWaitingMissionOnThisBay()
        {
            var retValue = false;

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay != null)
            {
                // Only applied for internal double bay
                if (bay.IsDouble && bay.Carousel == null && !bay.IsExternal)
                {
                    // List of waiting mission on the bay
                    var waitMissions = this.MissionsDataProvider.GetAllMissions()
                        .Where(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            ((m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick)
                            || (m.Status == MissionStatus.New && bay.Positions.Any(p => p.LoadingUnit?.Id == m.LoadUnitId)))
                        );

                    retValue = waitMissions.Any();
                }
            }

            return retValue;
        }

        public bool CheckMissionShowError()
        {
            if (this.Mission.ErrorCode != MachineErrorCode.NoError)
            {
                var loadUnit = this.LoadingUnitsDataProvider.GetById(this.Mission.LoadUnitId);
                this.ErrorsProvider.RecordNew(this.Mission.ErrorCode,
                    this.Mission.TargetBay,
                    string.Format(Resources.Missions.ErrorMissionDetails,
                        this.Mission.LoadUnitId,
                        Math.Round(loadUnit.GrossWeight - loadUnit.Tare),
                        Math.Round(loadUnit.Height),
                        this.Mission.WmsId ?? 0));

                //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, true);
                return true;
            }
            return false;
        }

        public bool CheckMissionShowError(Mission mission)
        {
            if (mission.ErrorCode != MachineErrorCode.NoError)
            {
                var loadUnit = this.LoadingUnitsDataProvider.GetById(mission.LoadUnitId);
                this.ErrorsProvider.RecordNew(mission.ErrorCode,
                    mission.TargetBay,
                    string.Format(Resources.Missions.ErrorMissionDetails,
                        mission.LoadUnitId,
                        Math.Round(loadUnit.GrossWeight - loadUnit.Tare),
                        Math.Round(loadUnit.Height),
                        mission.WmsId ?? 0));

                //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                this.BaysDataProvider.Light(mission.TargetBay, true);
                this.BaysDataProvider.CheckIntrusion(mission.TargetBay, true);
                return true;
            }
            return false;
        }

        public bool EnterErrorState(MissionStep errorState)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            this.Mission.Step = errorState;
            this.Mission.StepTime = DateTime.UtcNow;
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
                            if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell)
                            {
                                this.MachineProvider.UpdateServiceStatistics();
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            this.Logger.LogError($"SetLoadingUnit: Load Unit {this.Mission.LoadUnitId}; error {ex.Message}");
                            this.ErrorsProvider.RecordNew(MachineErrorCode.CellLogicallyOccupied, this.Mission.TargetBay, ex.Message);
                            throw new StateMachineException(ErrorDescriptions.CellLogicallyOccupied, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitSourceCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
                else if (this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
                {
                    var bayPosition = this.BaysDataProvider.GetPositionByLocation(this.Mission.LoadUnitSource);
                    this.BaysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                    this.MachineProvider.UpdateBayLoadUnitStatistics(this.Mission.TargetBay, this.Mission.LoadUnitId);
                    this.MachineProvider.UpdateServiceStatistics();
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

            if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell
                && this.Mission.LoadUnitId > 0
                )
            {
                // if we load from bay and load unit height is not compliant with the bay we go back
                var sourceBay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (sourceBay != null
                    && (this.Mission.MissionType != MissionType.Manual || this.Mission.MissionType != MissionType.ScaleCalibration)
                    && (!this.CheckBayHeight(sourceBay, this.Mission.LoadUnitSource, this.Mission, out var canRetry, out var errorCode)
                        //|| true    // TEST
                        )
                    )
                {
                    this.Mission.ErrorCode = errorCode;
                    this.MoveBackToBay();
                    return;
                }
                try
                {
                    this.Logger.LogDebug($"Height ok for LU {this.Mission.LoadUnitId}");
                    this.Mission.DestinationCellId = this.CellsProvider.FindEmptyCell(this.Mission.LoadUnitId, Utils.Enumerations.CompactingType.NoCompacting, isCellTest: (this.Mission.MissionType == MissionType.FirstTest), this.MachineVolatileDataProvider.RandomCells);
                    this.Logger.LogDebug($"Found cell {this.Mission.DestinationCellId} for LU {this.Mission.LoadUnitId}");
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.Mission.ErrorCode = MachineErrorCode.WarehouseIsFull;
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
            //else if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
            //{
            //    // optimizing movements: try to bypass a mission step
            //    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
            //    if (bay != null)
            //    {
            //        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
            //        throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
            //    }
            //    var loadUnitInBay = bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitSource)?.LoadingUnit;
            //    if (this.SensorsProvider.IsLoadingUnitInLocation(this.Mission.LoadUnitSource)
            //        && (loadUnitInBay is null
            //            || loadUnitInBay.Id == this.Mission.LoadUnitId
            //            )
            //        )
            //    {
            //        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotRemoved, this.Mission.TargetBay);
            //        throw new StateMachineException(ErrorDescriptions.LoadUnitNotRemoved, this.Mission.TargetBay, MessageActor.MachineManager);
            //    }

            //    var lowerUnit = bay.Positions.FirstOrDefault(p => !p.IsUpper && p.LoadingUnit != null)?.LoadingUnit;
            //    if (bay.Carousel != null
            //        && (
            //            lowerUnit is null
            //            || this.MissionsDataProvider.GetAllActiveMissions().FirstOrDefault(m => m.LoadUnitId == lowerUnit.Id) is null
            //            )
            //        )
            //    {
            //        var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            //        newStep.OnEnter(null);
            //    }
            //    else
            //    {
            //        var newStep = new MissionMoveWaitChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            //        newStep.OnEnter(null);
            //    }
            //}
            else
            {
                var newStep = new MissionMoveWaitChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public abstract void OnCommand(CommandMessage command);

        public abstract bool OnEnter(CommandMessage command, bool showErrors = true);

        public void OnHomingNotification(HomingMessageData messageData)
        {
            if ((messageData.AxisToCalibrate == Axis.Horizontal || messageData.AxisToCalibrate == Axis.HorizontalAndVertical)
                && this.Mission.NeedHomingAxis == messageData.AxisToCalibrate
                && !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)
                )
            {
                this.Mission.NeedHomingAxis = Axis.None;
                this.MissionsDataProvider.Update(this.Mission);
                this.MachineVolatileDataProvider.IsHomingExecuted = true;
            }
            else if (messageData.AxisToCalibrate == Axis.BayChain
                    && this.Mission.NeedHomingAxis == Axis.BayChain
                )
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (bay != null)
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
                if (reason == StopRequestReason.Abort
                    || !this.Mission.IsRestoringType()
                    || this.Mission.Step == MissionStep.New
                    || this.Mission.Step == MissionStep.NotDefined
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
                        if (this.Mission.NeedHomingAxis != Axis.HorizontalAndVertical)
                        {
                            this.Mission.NeedHomingAxis = Axis.Horizontal;
                        }
                        newStep = new MissionMoveErrorLoadStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else if (this.Mission.Step == MissionStep.DepositUnit)
                    {
                        if (this.Mission.NeedHomingAxis != Axis.HorizontalAndVertical)
                        {
                            this.Mission.NeedHomingAxis = Axis.Horizontal;
                        }
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
            var update = false;
            switch (type)
            {
                case MessageType.CombinedMovements:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.CombinedMovements;
                    update = true;
                    break;

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

                case MessageType.CheckIntrusion:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.CheckIntrusion;
                    update = true;
                    break;
            }
            return update;
        }

        private void MoveBackToBay()
        {
            this.Mission.LoadUnitDestination = this.Mission.LoadUnitSource;
            this.MissionsDataProvider.Update(this.Mission);

            if (!this.isWaitingMissionOnThisBay())
            {
                var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (bay != null)
                {
                    // Only applied for internal double bay
                    if (bay.IsDouble && bay.Carousel == null && !bay.IsExternal)
                    {
                        switch (this.Mission.ErrorCode)
                        {
                            case MachineErrorCode.LoadUnitHeightFromBayExceeded:
                            case MachineErrorCode.LoadUnitHeightFromBayTooLow:
                                var newStep1 = new MissionMoveBackToBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep1.OnEnter(null);
                                break;

                            default:
                                var newStep2 = new MissionMoveWaitDepositBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep2.OnEnter(null);
                                break;
                        }

                        return;
                    }
                }

                var newStep = new MissionMoveWaitDepositBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
