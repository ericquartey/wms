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
            }

            return returnValue;
        }

        public override void OnNotification(NotificationMessage message)
        {
            // do nothing
        }

        private bool CheckBayDestination(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, LoadingUnitLocation destination, Mission mission, bool showErrors = true)
        {
            bool returnValue = true;
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
            else if ((bay = this.BaysDataProvider.GetByLoadingUnitLocation(destination)) != null
                && bay.Shutter.Type != ShutterType.NotSpecified
                && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Closed
                && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Opened
                )
            {
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterInvalid, requestingBay);
                }
                returnValue = false;
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
                        if (destinationBay.Shutter.Type != ShutterType.NotSpecified
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
                            && !this.CheckBayHeight(destinationBay, destination, mission)
                            )
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitHeightExceeded, requestingBay);
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
            bool returnValue = false;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.DestinationCellId != null)
                    {
                        returnValue = this.CellsProvider.CanFitLoadingUnit(messageData.DestinationCellId.Value, this.Mission.LoadUnitId);
                        mission.DestinationCellId = messageData.DestinationCellId;
                        mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                    }
                    else if (messageData.LoadUnitId.HasValue)
                    {
                        try
                        {
                            mission.DestinationCellId = this.CellsProvider.FindEmptyCell(messageData.LoadUnitId.Value);
                        }
                        catch (Exception)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.WarehouseIsFull, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        returnValue = true;
                        mission.LoadUnitDestination = LoadingUnitLocation.Cell;
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
                        throw new StateMachineException(ErrorDescriptions.DestinationTypeNotValid, this.Mission.TargetBay, MessageActor.MachineManager);
                    }

                default:
                    // destination is bay, but first we must decide which position to use
                    var bay = this.BaysDataProvider.GetByNumber(requestingBay);
                    if (bay.Positions.Count() == 1)
                    {
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, mission, showErrors);
                        if (returnValue)
                        {
                            mission.LoadUnitDestination = bay.Positions.First().Location;
                        }
                    }
                    else
                    {
                        if (bay.Carousel != null
                            && !this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number])
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.DestinationBayNotCalibrated, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.DestinationBayNotCalibrated, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.DestinationBayNotCalibrated);
                                return false;
                            }
                        }
                        var upper = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (upper is LoadingUnitLocation.NoLocation)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitUndefinedUpper);
                                return false;
                            }
                        }
                        var bottom = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (bottom is LoadingUnitLocation.NoLocation)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedBottom, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitUndefinedBottom);
                                return false;
                            }
                        }
                        // always check upper position first
                        returnValue = this.CheckBayDestination(messageData, requestingBay, upper, mission, false || messageData.Destination == upper);
                        if (returnValue)
                        {
                            // upper position is empty. we can use it only if bottom is also free
                            returnValue = this.CheckBayDestination(messageData, requestingBay, bottom, mission, showErrors);
                            if (returnValue)
                            {
                                // both positions are free: choose upper if not fixed by message
                                if (messageData.Destination == bottom)
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
                                if (showErrors)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                                }
                                else
                                {
                                    this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                    return false;
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
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                                }
                                else
                                {
                                    this.Logger.LogInformation(ErrorDescriptions.LoadUnitDestinationBay);
                                    return false;
                                }
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
            bool returnValue = false;
            switch (messageData.MissionType)
            {
                case MissionType.Manual:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Manual);
                    break;

                case MissionType.LoadUnitOperation:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.LoadUnitOperations
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToLoadUnitOperations
                        );
                    break;

                case MissionType.Compact:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Compact
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToCompact
                        );
                    break;

                case MissionType.WMS:
                case MissionType.OUT:
                case MissionType.IN:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Automatic
                        || this.MachineVolatileDataProvider.Mode == MachineMode.SwitchingToAutomatic
                        );
                    break;

                case MissionType.FullTest:
                    returnValue = (this.MachineVolatileDataProvider.Mode == MachineMode.Test);
                    break;

                default:
                    this.ErrorsProvider.RecordNew(MachineErrorCode.MissionTypeNotValid, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.MissionTypeNotValid, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            if (!returnValue)
            {
                this.Logger.LogDebug($"IsMachineOk: Machine Mode {this.MachineVolatileDataProvider.Mode} not valid for mission type {messageData.MissionType}");
                this.ErrorsProvider.RecordNew(MachineErrorCode.MachineModeNotValid, this.Mission.TargetBay);
                return false;
            }

            var activeMission = this.MissionsDataProvider.GetAllActiveMissionsByBay(this.Mission.TargetBay)
                .FirstOrDefault(x => x.Status == MissionStatus.Executing);

            if (activeMission != null)
            {
                this.Logger.LogTrace($"IsMachineOk: waiting for active mission {activeMission.Id}, LoadUnit {activeMission.LoadUnitId}; bay {this.Mission.TargetBay}");
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.AnotherMissionIsActiveForThisBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.AnotherMissionIsActiveForThisBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.Logger.LogInformation(ErrorDescriptions.AnotherMissionIsActiveForThisBay);
                    return false;
                }
            }

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
                        unitToMove = sourceCell.LoadingUnit;

                        mission.LoadUnitSource = LoadingUnitLocation.Cell;
                        mission.LoadUnitCellSourceId = sourceCell.Id;

                        bay = this.BaysDataProvider.GetByCell(sourceCell);
                        if (bay != null
                            && bay.Shutter.Type != ShutterType.NotSpecified)
                        {
                            var shutterInverter = bay.Shutter.Inverter.Index;
                            if (this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed)
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
                            mission.LoadUnitSource = LoadingUnitLocation.Cell;
                            mission.LoadUnitCellSourceId = sourceCell.Id;
                        }
                        else
                        {
                            var sourceBay = this.BaysDataProvider.GetLoadingUnitLocationByLoadingUnit(unitToMove.Id);

                            if (sourceBay != LoadingUnitLocation.NoLocation)
                            {
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
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotLoaded, requestingBay);
                            }
                            else
                            {
                                this.Logger.LogInformation(ErrorDescriptions.LoadUnitNotLoaded);
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
                    else if ((bay = this.BaysDataProvider.GetByLoadingUnitLocation(messageData.Source)) != null
                        && bay.Shutter.Type != ShutterType.NotSpecified
                        && messageData.InsertLoadUnit
                        && this.SensorsProvider.GetShutterPosition(bay.Shutter.Inverter.Index) != ShutterPosition.Closed)
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
