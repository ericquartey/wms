using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveNewStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveNewStep(Mission mission,
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

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            var returnValue = this.CheckStartConditions(this.Mission, command, showErrors);
            if (returnValue)
            {
                this.Mission.Status = MissionStatus.New;

                if (command != null
                    && command.Data is IMoveLoadingUnitMessageData messageData
                    )
                {
                    this.Mission.Action = messageData.CommandAction;
                    this.Mission.TargetBay = command.RequestingBay;
                }
                this.MissionsDataProvider.Update(this.Mission);
                this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

                var startState = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                returnValue = startState.OnEnter(null);
            }
            else if (showErrors)
            {
                this.MissionsDataProvider.Delete(this.Mission.Id);
            }
            else
            {
                this.MissionsDataProvider.Reload(this.Mission);
                //if (this.Mission.Step == MissionStep.Error && this.Mission.Status == MissionStatus.New)
                //{
                //    this.Mission.Step = MissionStep.NotDefined;
                //    this.MissionsDataProvider.Update(this.Mission);
                //    this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
                //}
            }

            return returnValue;
        }

        public override void OnNotification(NotificationMessage message)
        {
            // do nothing
        }

        private bool CheckBayDestination(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, LoadingUnitLocation destination, Mission mission, bool showErrors = true)
        {
            var returnValue = true;
            Bay bay;
#if CHECK_BAY_SENSOR
            if (this.SensorsProvider.IsLoadingUnitInLocation(destination))
            {
                returnValue = false;
            }
            else
#endif
            {
                returnValue = (messageData.Source == destination)
                    || (this.BaysDataProvider.GetLoadingUnitByDestination(destination) == null);
            }
            if (!returnValue)
            {
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, requestingBay);
                }
            }
            else
            {
                if ((bay = this.BaysDataProvider.GetByLoadingUnitLocation(destination)) != null
                      && bay.Shutter != null
                      && bay.Shutter.Type != ShutterType.NotSpecified
                      && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Closed
                      && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Half
                      )
                {
                    if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterInvalid, requestingBay);
                    }
                    returnValue = false;
                }
            }

            if (returnValue &&
                (bay = this.BaysDataProvider.GetByLoadingUnitLocation(destination)) != null &&
                bay.IsExternal)
            {
                if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number) ||
                    this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                {
                    if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, requestingBay);
                    }
                    returnValue = false;
                }
            }

            if (returnValue
                && destination != LoadingUnitLocation.NoLocation)
            {
                var destinationBay = this.BaysDataProvider.GetByLoadingUnitLocation(destination);
                if (destinationBay != null)
                {
                    if (mission.LoadUnitSource != LoadingUnitLocation.Elevator
                        && mission.LoadUnitCellSourceId == null)
                    {
                        // move from bay to bay
                        if (destinationBay.Shutter != null
                            && destinationBay.Shutter.Type != ShutterType.NotSpecified
                            && this.SensorsProvider.GetShutterPosition(destinationBay.Shutter.Inverter.Index) != ShutterPosition.Closed
                            && this.SensorsProvider.GetShutterPosition(destinationBay.Shutter.Inverter.Index) != ShutterPosition.Opened
                            )
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterInvalid, requestingBay);
                            }
                            returnValue = false;
                        }
                    }
                    else
                    {
                        if (mission.MissionType != MissionType.Manual
                            && !this.CheckBayHeight(destinationBay, destination, mission, out var canRetry)
                            )
                        {
                            if (showErrors || !canRetry)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitHeightToBayExceeded, requestingBay);
                                if (!showErrors)
                                {
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitHeightToBayExceeded, requestingBay, MessageActor.MachineManager);
                                }
                            }
                            returnValue = false;
                        }
                        if (returnValue
                            && destinationBay.Positions.FirstOrDefault(b => b.Location == destination).IsBlocked)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.BayPositionDisabled, requestingBay);
                            }
                            returnValue = false;
                        }
                    }
                }
            }

            return returnValue;
        }

        // it checks message data and updates mission
        private bool CheckStartConditions(Mission mission, CommandMessage commandMessage, bool showErrors)
        {
            bool returnValue;

            if (commandMessage != null
                && commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                returnValue = this.IsMachineOk(messageData, showErrors);

                if (returnValue)
                {
                    returnValue = this.IsSourceOk(mission, messageData, commandMessage.RequestingBay, showErrors);
                }

                if (returnValue)
                {
                    returnValue = this.IsDestinationOk(mission, messageData, commandMessage.RequestingBay, showErrors);
                }

                if (returnValue)
                {
                    returnValue = this.IsElevatorOk(mission, showErrors);
                }
            }
            else
            {
                var description = $"Attempting to start {this.GetType()} Finite state machine with wrong ({commandMessage?.Data.GetType()}) message data";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }

            return returnValue;
        }

        private bool IsDestinationOk(Mission mission, IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, bool showErrors)
        {
            var returnValue = false;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.DestinationCellId != null)
                    {
                        returnValue = this.CellsProvider.CanFitLoadingUnit(messageData.DestinationCellId.Value, this.Mission.LoadUnitId, (this.Mission.MissionType == MissionType.FirstTest));
                        mission.DestinationCellId = messageData.DestinationCellId;
                        mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                    }
                    else if (messageData.LoadUnitId.HasValue)
                    {
                        try
                        {
                            mission.DestinationCellId = this.CellsProvider.FindEmptyCell(messageData.LoadUnitId.Value);
                            returnValue = true;
                            mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                        }
                        catch (Exception ex)
                        {
                            if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell
                                || this.Mission.LoadUnitSource == LoadingUnitLocation.Elevator
                                )
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull, this.Mission.TargetBay, ex.Message);
                            }
                            else
                            {
                                returnValue = true;
                                mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                            }
                        }
                    }

                    if (!returnValue)
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationCell);
                        }
                    }

                    break;

                case LoadingUnitLocation.Elevator:
                    mission.LoadUnitDestination = LoadingUnitLocation.Elevator;
                    returnValue = true;
                    break;

                case LoadingUnitLocation.LoadUnit:
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.DestinationTypeNotValid, this.Mission.TargetBay);
                        break;
                    }

                default:
                    // destination is bay, but first we must decide which position to use
                    var bay = this.BaysDataProvider.GetByNumber(requestingBay);
                    if (bay.Positions.Count() == 1)
                    {
                        if (bay.External != null &&
                            !this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number])
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.DestinationBayNotCalibrated, this.Mission.TargetBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.DestinationBayNotCalibrated);
                            }
                            return false;
                        }
                        // always check upper position
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, mission, showErrors);
                        if (returnValue)
                        {
                            mission.LoadUnitDestination = bay.Positions.First().Location;
                        }
                    }
                    else
                    {
                        if (bay.Carousel != null
                            && !this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number]
                            && bay.Positions.Any(p => p.LoadingUnit != null)
                            )
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.DestinationBayNotCalibrated, this.Mission.TargetBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.DestinationBayNotCalibrated);
                            }
                            return false;
                        }
                        var upper = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (upper is LoadingUnitLocation.NoLocation)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitUndefinedUpper);
                            }
                            return false;
                        }
                        var bottom = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (bottom is LoadingUnitLocation.NoLocation)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitUndefinedBottom);
                            }
                            return false;
                        }

                        // Previous code ....
                        //// always check upper position first
                        //returnValue = this.CheckBayDestination(messageData, requestingBay, upper, mission, false || messageData.Destination == upper);
                        //if (returnValue)
                        //{
                        //    // upper position is empty. we can use it only if bottom is also free
                        //    returnValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                        //    if (returnValue
                        //        || bay.Positions.FirstOrDefault(b => b.Location == bottom).IsBlocked
                        //        )
                        //    {
                        //        returnValue = true;
                        //        // both positions are free: choose upper if not fixed by message
                        //        if (messageData.Destination == bottom && !bay.Positions.FirstOrDefault(b => b.Location == bottom).IsBlocked)
                        //        {
                        //            mission.LoadUnitDestination = bottom;
                        //        }
                        //        else
                        //        {
                        //            mission.LoadUnitDestination = upper;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (showErrors
                        //            || !bay.Positions.Any(p => p.LoadingUnit is null && !p.IsBlocked)
                        //            )
                        //        {
                        //            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                        //        }
                        //        else
                        //        {
                        //            this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                        //        }
                        //        return false;
                        //    }
                        //}
                        //else
                        //{
                        //    if (messageData.Destination == upper)
                        //    {
                        //        if (showErrors)
                        //        {
                        //            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                        //        }
                        //        else
                        //        {
                        //            this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                        //        }
                        //        return false;
                        //    }
                        //    // if upper position is not empty we can try lower position
                        //    returnValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                        //    if (returnValue)
                        //    {
                        //        // upper is occupied and bottom is free: choose bottom
                        //        mission.LoadUnitDestination = bottom;
                        //    }
                        //}

                        if (!bay.IsDouble)
                        {
                            // always check upper position first
                            returnValue = this.CheckBayDestination(messageData, requestingBay, upper, mission, false || messageData.Destination == upper);
                            if (returnValue)
                            {
                                // upper position is empty. we can use it only if bottom is also free
                                returnValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                                if (returnValue
                                    || bay.Positions.FirstOrDefault(b => b.Location == bottom).IsBlocked
                                    )
                                {
                                    returnValue = true;
                                    // both positions are free: choose upper if not fixed by message
                                    if (messageData.Destination == bottom && !bay.Positions.FirstOrDefault(b => b.Location == bottom).IsBlocked)
                                    {
                                        mission.LoadUnitDestination = bottom;
                                    }
                                    else
                                    {
                                        mission.LoadUnitDestination = upper;
                                    }
                                }
                                else
                                {
                                    if (showErrors
                                        || !bay.Positions.Any(p => p.LoadingUnit is null && !p.IsBlocked)
                                        )
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                if (messageData.Destination == upper)
                                {
                                    if (showErrors)
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                    }
                                    return false;
                                }
                                // if upper position is not empty we can try lower position
                                returnValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                                if (returnValue)
                                {
                                    // upper is occupied and bottom is free: choose bottom
                                    mission.LoadUnitDestination = bottom;
                                }
                            }
                        }

                        if (bay.IsDouble)
                        {
                            returnValue = true;
                            // always check upper position first
                            var bValue = this.CheckBayDestination(messageData, requestingBay, upper, mission, /*false || messageData.Destination == upper*/showErrors);
                            if (bValue)
                            {
                                // upper position is empty. we can use it only if bottom is also free
                                bValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                                if (bValue)
                                {
                                    // we choose the upper position
                                    // both positions are free: choose upper if not fixed by message
                                    mission.LoadUnitDestination = upper;

                                    if (messageData.Destination == bottom)
                                    {
                                        mission.LoadUnitDestination = bottom;
                                    }
                                }
                                else
                                {
                                    if (messageData.Destination == upper)
                                    {
                                        if (showErrors)
                                        {
                                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                                        }
                                        else
                                        {
                                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                        }
                                        returnValue = false;
                                    }
                                    else
                                    {
                                        // we choose definitely the upper position
                                        mission.LoadUnitDestination = upper;
                                    }
                                }
                            }
                            else
                            {
                                if (messageData.Destination == upper)
                                {
                                    if (showErrors)
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                    }
                                    returnValue = false;
                                }
                                else
                                {
                                    // we can use it only if bottom is also free
                                    bValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                                    if (bValue)
                                    {
                                        // we choose definitely the lower position
                                        mission.LoadUnitDestination = bottom;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return returnValue;
        }

        private bool IsElevatorOk(Mission mission, bool showErrors)
        {
            var returnValue = false;

            if (mission.LoadUnitSource == LoadingUnitLocation.Elevator)
            {
                returnValue = this.ElevatorDataProvider.GetLoadingUnitOnBoard() != null;
                if (!returnValue)
                {
                    if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceElevator, this.Mission.TargetBay);
                    }
                    else
                    {
                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitSourceElevator);
                        return false;
                    }
                }
                else
                {
                    returnValue = this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator, this.Mission.TargetBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitPresentOnEmptyElevator);
                            return false;
                        }
                    }
                }
            }
            else
            {
                returnValue = this.ElevatorDataProvider.GetLoadingUnitOnBoard() == null;

                if (!returnValue)
                {
                    if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitElevator, this.Mission.TargetBay);
                    }
                    else
                    {
                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitElevator);
                        return false;
                    }
                }
                else
                {
                    returnValue = !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator, this.Mission.TargetBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitPresentOnEmptyElevator);
                            return false;
                        }
                    }
                }
            }

            return returnValue;
        }

        private bool IsMachineOk(IMoveLoadingUnitMessageData messageData, bool showErrors)
        {
            var returnValue = false;
            switch (messageData.MissionType)
            {
                case MissionType.Manual:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Manual
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToManual
                        );
                    break;

                case MissionType.LoadUnitOperation:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.LoadUnitOperations
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations
                        || this.MachineVolatileDataProvider.Mode == MachineMode.Manual
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToManual
                        );
                    break;

                case MissionType.Compact:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Compact
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToManual
                        );
                    break;

                case MissionType.WMS:
                case MissionType.OUT:
                case MissionType.IN:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Automatic
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic
                        );
                    break;

                case MissionType.FullTestIN:
                case MissionType.FullTestOUT:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.FullTest
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToFullTest
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToManual
                        );
                    break;

                case MissionType.FirstTest:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.FirstTest
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToFirstTest
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToManual
                        );
                    break;

                default:
                    this.ErrorsProvider.RecordNew(MachineErrorCode.MissionTypeNotValid, this.Mission.TargetBay);
                    break;
            }
            if (!returnValue)
            {
                this.Logger.LogDebug($"IsMachineOk: Machine Mode {this.MachineVolatileDataProvider.Mode} not valid for mission type {messageData.MissionType}");
                this.ErrorsProvider.RecordNew(MachineErrorCode.MachineModeNotValid, this.Mission.TargetBay);
                return false;
            }

            var activeMission = this.MissionsDataProvider.GetAllActiveMissions()
                .FirstOrDefault(x => x.Status == MissionStatus.Executing);

            if (activeMission != null)
            {
                this.Logger.LogTrace($"IsMachineOk: waiting for active Mission:Id={activeMission.Id}, Load Unit {activeMission.LoadUnitId}; bay {this.Mission.TargetBay}");
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, this.Mission.TargetBay);
                }
                else
                {
                    this.Logger.LogInformation($"{ErrorReasons.AnotherMissionIsActiveForThisBay}. Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                }
                return false;
            }

            //if (activeMission != null)
            //{
            //    this.Logger.LogTrace($"IsMachineOk: waiting for active Mission:Id={activeMission.Id}, Load Unit {activeMission.LoadUnitId}; bay {this.Mission.TargetBay}");

            //    var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
            //    if (!bay.IsDouble)
            //    {
            //        if (showErrors)
            //        {
            //            this.ErrorsProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, this.Mission.TargetBay);
            //        }
            //        else
            //        {
            //            this.Logger.LogInformation($"{ErrorReasons.AnotherMissionIsActiveForThisBay}. Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
            //        }
            //        return false;
            //    }

            //    if (bay.IsDouble)
            //    {
            //        if (activeMission.LoadUnitDestination != this.Mission.LoadUnitDestination)
            //        {
            //            this.Logger.LogDebug($"A waiting mission is already here :>: {activeMission}");
            //            return true;
            //        }
            //        else
            //        {
            //            if (showErrors)
            //            {
            //                this.ErrorsProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, this.Mission.TargetBay);
            //            }
            //            else
            //            {
            //                this.Logger.LogInformation($"{ErrorReasons.AnotherMissionIsActiveForThisBay}. Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
            //            }
            //            return false;
            //        }
            //    }
            //}

#if !CHECK_BAY_SENSOR
            return true;
#else
            else
            {
                returnValue = this.SensorsProvider.IsLoadingUnitInLocation(messageData.Source);
            }
            if (!returnValue && showErrors)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.NoLoadUnitInSource);
            }

            return returnValue;
#endif
        }

        private bool IsSourceOk(Mission mission, IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, bool showErrors)
        {
            LoadingUnit unitToMove = null;
            Bay bay;

            switch (messageData.Source)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.SourceCellId != null)
                    {
                        var sourceCell = this.CellsProvider.GetById(messageData.SourceCellId.Value);
                        if (sourceCell.BlockLevel != BlockLevel.Blocked)
                        {
                            unitToMove = sourceCell.LoadingUnit;
                            mission.LoadUnitSource = LoadingUnitLocation.Cell;
                            mission.LoadUnitCellSourceId = sourceCell.Id;

                            bay = this.BaysDataProvider.GetByCell(sourceCell);
                            if (bay != null
                                && bay.Shutter != null
                                && bay.Shutter.Type != ShutterType.NotSpecified)
                            {
                                var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                                var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                                if (shutterPosition != ShutterPosition.Closed
                                    && shutterPosition != ShutterPosition.Half
                                    )
                                {
                                    if (showErrors)
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, bay.Number);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitShutterOpen);
                                    }
                                    return false;
                                }
                            }
                        }
                    }

                    if (unitToMove == null)
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, requestingBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitSourceCell);
                        }
                        return false;
                    }
                    break;

                case LoadingUnitLocation.LoadUnit:
                    if (messageData.LoadUnitId != null)
                    {
                        unitToMove = this.LoadingUnitsDataProvider.GetById(messageData.LoadUnitId.Value);
                    }

                    if (unitToMove != null)
                    {
                        var sourceCell = this.CellsProvider.GetByLoadingUnitId(unitToMove.Id);

                        if (sourceCell != null)
                        {
                            if (!messageData.InsertLoadUnit)
                            {
                                if (sourceCell.BlockLevel == BlockLevel.Blocked)
                                {
                                    if (showErrors)
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, requestingBay);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitSourceCell);
                                    }
                                    return false;
                                }
                                else
                                {
                                    mission.LoadUnitSource = LoadingUnitLocation.Cell;
                                    mission.LoadUnitCellSourceId = sourceCell.Id;
                                }
                            }
                        }
                        else
                        {
                            var sourceBay = this.BaysDataProvider.GetLoadingUnitLocationByLoadingUnit(unitToMove.Id);

                            if (sourceBay != LoadingUnitLocation.NoLocation)
                            {
                                bay = this.BaysDataProvider.GetByLoadingUnitLocation(sourceBay);
                                if (bay != null &&
                                    bay.Positions.FirstOrDefault(b => b.Location == sourceBay).IsBlocked)
                                {
                                    if (showErrors)
                                    {
                                        this.ErrorsProvider.RecordNew(MachineErrorCode.BayPositionDisabled, requestingBay);
                                    }
                                    else
                                    {
                                        this.Logger.LogInformation(ErrorDescriptions.BayPositionDisabled);
                                    }
                                    return false;
                                }
                                mission.LoadUnitSource = sourceBay;
                            }
                            else
                            {
                                // it is not in bay and not in cell, maybe it is on the elevator?
                                var elevatorLoadUnit = this.ElevatorDataProvider.GetLoadingUnitOnBoard();
                                if (elevatorLoadUnit is null
                                    || unitToMove.Id != elevatorLoadUnit.Id)
                                {
                                    unitToMove = null;
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotLoaded, requestingBay);
                                }
                                else
                                {
                                    mission.LoadUnitSource = LoadingUnitLocation.Elevator;
                                }
                            }
                        }

                        if (mission.LoadUnitSource == LoadingUnitLocation.NoLocation)
                        {
                            unitToMove = null;
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew((messageData.InsertLoadUnit) ? MachineErrorCode.LoadUnitNotFound : MachineErrorCode.LoadUnitNotLoaded, requestingBay);
                            }
                            else
                            {
                                this.Logger.LogInformation((messageData.InsertLoadUnit) ? ErrorDescriptions.LoadUnitNotFound : ErrorDescriptions.LoadUnitNotLoaded);
                            }
                        }
                    }
                    else if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, requestingBay);
                    }
                    else
                    {
                        this.Logger.LogInformation(ErrorDescriptions.LoadUnitNotFound);
                    }
                    if (unitToMove == null)
                    {
                        return false;
                    }

                    break;

                default:
                    if (messageData.InsertLoadUnit)
                    {
                        if (messageData.LoadUnitId.HasValue)
                        {
                            var bayPosition = this.BaysDataProvider.GetLoadingUnitLocationByLoadingUnit(messageData.LoadUnitId.Value);
                            if (bayPosition == LoadingUnitLocation.NoLocation
                                || bayPosition == messageData.Source
                                )
                            {
                                unitToMove = this.LoadingUnitsDataProvider.GetById(messageData.LoadUnitId.Value);
                            }
                            else if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitOtherBay, requestingBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitOtherBay);
                            }
                        }
                    }
                    else
                    {
                        unitToMove = this.BaysDataProvider.GetLoadingUnitByDestination(messageData.Source);
                    }

                    if (unitToMove == null)
                    {
                        unitToMove = null;
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, requestingBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitNotFound);
                        }
                    }
                    else if (unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentInCell, requestingBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitPresentInCell);
                        }
                    }
#if CHECK_BAY_SENSOR
                    else if (!this.SensorsProvider.IsLoadingUnitInLocation(messageData.Source))
                    {
                        unitToMove = null;
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, requestingBay);
                        }
                        else
                        {
                            this.Logger.LogInformation(ErrorDescriptions.LoadUnitSourceBay);
                        }
                    }
#endif
                    else if ((bay = this.BaysDataProvider.GetByLoadingUnitLocation(messageData.Source)) != null)
                    {
                        if (bay.Shutter != null
                            && bay.Shutter.Type != ShutterType.NotSpecified
                            && messageData.InsertLoadUnit
                            && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Closed
                            && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Half
                            )
                        {
                            unitToMove = null;
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitShutterOpen);
                            }
                        }
                        else if (bay.Positions.FirstOrDefault(b => b.Location == messageData.Source).IsBlocked)
                        {
                            unitToMove = null;
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.BayPositionDisabled, requestingBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.BayPositionDisabled);
                            }
                        }
                    }

                    if (unitToMove == null)
                    {
                        return false;
                    }

                    mission.LoadUnitSource = messageData.Source;
                    break;
            }

            if (unitToMove != null)
            {
                mission.LoadUnitId = unitToMove.Id;
            }
            else
            {
                unitToMove = null;
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceDb, requestingBay);
                }
                else
                {
                    this.Logger.LogInformation(ErrorDescriptions.LoadUnitSourceDb);
                }
            }

            return unitToMove != null;
        }

        #endregion
    }
}
