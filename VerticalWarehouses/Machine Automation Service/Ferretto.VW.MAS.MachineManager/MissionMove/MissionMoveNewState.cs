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
    public class MissionMoveNewState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveNewState(Mission mission,
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

        public override bool OnEnter(CommandMessage command)
        {
            var returnValue = this.CheckStartConditions(this.Mission, command);
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

                var startState = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                returnValue = startState.OnEnter(null);
            }
            else
            {
                this.MissionsDataProvider.Delete(this.Mission.Id);
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
#if CHECK_BAY_SENSOR
            if (!this.SensorsProvider.IsLoadingUnitInLocation(destination))
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
            else if (this.BaysDataProvider.GetByLoadingUnitLocation(destination).Shutter.Type != ShutterType.NotSpecified
                && this.SensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed
                && this.SensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Opened
                )
            {
                if (showErrors)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
                }
                returnValue = false;
            }
            if (returnValue
                && destination != LoadingUnitLocation.NoLocation)
            {
                var destinationBay = this.BaysDataProvider.GetByLoadingUnitLocation(destination);
                if (destinationBay != null
                    && destinationBay.Number != requestingBay
                    )
                {
                    // move from bay to bay
                    if (this.BaysDataProvider.GetByLoadingUnitLocation(destination).Shutter.Type != ShutterType.NotSpecified
                        && this.SensorsProvider.GetShutterPosition(destinationBay.Number) != ShutterPosition.Closed
                        && this.SensorsProvider.GetShutterPosition(destinationBay.Number) != ShutterPosition.Opened
                        )
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
                        }
                        returnValue = false;
                    }
                }
            }
            if (returnValue)
            {
                mission.LoadUnitDestination = destination;
            }

            return returnValue;
        }

        // it checks message data and updates mission
        private bool CheckStartConditions(Mission mission, CommandMessage commandMessage)
        {
            bool returnValue;

            if (commandMessage != null
                && commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                returnValue = this.IsMachineOk(messageData);

                if (returnValue)
                {
                    returnValue = this.IsSourceOk(mission, messageData, commandMessage.RequestingBay);
                }

                if (returnValue)
                {
                    returnValue = this.IsDestinationOk(mission, messageData, commandMessage.RequestingBay);
                }

                if (returnValue)
                {
                    returnValue = this.IsElevatorOk(mission);
                }
            }
            else
            {
                var description = $"Attempting to start {this.GetType()} Finite state machine with wrong ({commandMessage?.Data.GetType()}) message data";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }

            return returnValue;
        }

        private bool IsDestinationOk(Mission mission, IMoveLoadingUnitMessageData messageData, BayNumber requestingBay)
        {
            bool returnValue = false;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.DestinationCellId != null)
                    {
                        var destinationCell = this.CellsProvider.GetById(messageData.DestinationCellId.Value);
                        returnValue = (destinationCell.LoadingUnit == null && destinationCell.IsFree);
                        mission.DestinationCellId = messageData.DestinationCellId;
                        mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                    }
                    else if (messageData.LoadingUnitId.HasValue)
                    {
                        try
                        {
                            mission.DestinationCellId = this.CellsProvider.FindEmptyCell(messageData.LoadingUnitId.Value);
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
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
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
                case LoadingUnitLocation.NoLocation:
                    // destination is bay, but first we must decide which position to use
                    var bay = this.BaysDataProvider.GetByNumber(requestingBay);
                    if (bay.Positions.Count() == 1)
                    {
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, mission);
                    }
                    else
                    {
                        var destination = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (destination is LoadingUnitLocation.NoLocation)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        returnValue = this.CheckBayDestination(messageData, requestingBay, destination, mission, false);
                        if (!returnValue)
                        {
                            // if upper position is not empty we can try lower position
                            destination = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                            if (destination is LoadingUnitLocation.NoLocation)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedBottom, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            // the other mission must be in waiting state
                            var activeMission = this.MissionsDataProvider.GetAllActiveMissionsByBay(requestingBay)
                                .FirstOrDefault(x => (x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting)
                                    && x.NeedHomingAxis != Axis.BayChain);

                            if (activeMission != null
                                && activeMission.Status != MissionStatus.Waiting
                                )
                            {
                                this.Logger.LogTrace($"IsDestinationOk: waiting for mission in upper position {activeMission.Id}, LoadUnit {activeMission.LoadUnitId}; bay {requestingBay}");
                            }
                            else
                            {
                                returnValue = this.CheckBayDestination(messageData, requestingBay, destination, mission);
                            }
                        }
                    }
                    break;

                default:
                    returnValue = this.CheckBayDestination(messageData, requestingBay, messageData.Destination, mission);

                    break;
            }

            return returnValue;
        }

        private bool IsElevatorOk(Mission mission)
        {
            var returnValue = false;

            if (mission.LoadUnitSource == LoadingUnitLocation.Elevator)
            {
                returnValue = this.ElevatorDataProvider.GetLoadingUnitOnBoard() != null;
                if (!returnValue)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceElevator, this.Mission.TargetBay);
                }
                else
                {
                    returnValue = this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator, this.Mission.TargetBay);
                    }
                }
            }
            else
            {
                returnValue = this.ElevatorDataProvider.GetLoadingUnitOnBoard() == null;

                if (!returnValue)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitElevator, this.Mission.TargetBay);
                }
                else
                {
                    returnValue = !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator, this.Mission.TargetBay);
                    }
                }
            }

            return returnValue;
        }

        private bool IsMachineOk(IMoveLoadingUnitMessageData messageData)
        {
            bool returnValue = false;
            switch (messageData.MissionType)
            {
                case MissionType.Manual:
                    returnValue = (this.MachineModeDataProvider.Mode == MachineMode.Manual);
                    break;

                case MissionType.Compact:
                    returnValue = (this.MachineModeDataProvider.Mode == MachineMode.Compact);
                    break;

                case MissionType.WMS:
                case MissionType.OUT:
                case MissionType.IN:
                    returnValue = (this.MachineModeDataProvider.Mode == MachineMode.Automatic);
                    break;

                case MissionType.Test:
                    returnValue = (this.MachineModeDataProvider.Mode == MachineMode.Test);
                    break;

                default:
                    this.ErrorsProvider.RecordNew(MachineErrorCode.MissionTypeNotValid, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.MissionTypeNotValid, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            if (!returnValue)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.MachineModeNotValid, this.Mission.TargetBay);
                return false;
            }

#if !CHECK_BAY_SENSOR
            return true;
#else
            else
            {
                returnValue = this.SensorsProvider.IsLoadingUnitInLocation(messageData.Source);
            }
            if (!returnValue)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.NoLoadUnitInSource);
            }

            return returnValue;
#endif
        }

        private bool IsSourceOk(Mission mission, IMoveLoadingUnitMessageData messageData, BayNumber requestingBay)
        {
            LoadingUnit unitToMove = null;

            switch (messageData.Source)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.SourceCellId != null)
                    {
                        var sourceCell = this.CellsProvider.GetById(messageData.SourceCellId.Value);
                        unitToMove = sourceCell.LoadingUnit;

                        mission.LoadUnitSource = LoadingUnitLocation.Cell;
                        mission.LoadUnitCellSourceId = sourceCell.Id;
                    }

                    if (unitToMove == null)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, requestingBay);
                        return false;
                    }

                    break;

                case LoadingUnitLocation.LoadUnit:
                    if (messageData.LoadingUnitId != null)
                    {
                        unitToMove = this.LoadingUnitsDataProvider.GetById(messageData.LoadingUnitId.Value);
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
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotLoaded, requestingBay);
                        }
                    }
                    else
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, requestingBay);
                    }
                    if (unitToMove == null)
                    {
                        return false;
                    }

                    break;

                default:
                    if (messageData.InsertLoadingUnit)
                    {
                        if (messageData.LoadingUnitId.HasValue)
                        {
                            var bayPosition = this.BaysDataProvider.GetLoadingUnitLocationByLoadingUnit(messageData.LoadingUnitId.Value);
                            if (bayPosition == LoadingUnitLocation.NoLocation
                                || bayPosition == messageData.Source
                                )
                            {
                                unitToMove = this.LoadingUnitsDataProvider.GetById(messageData.LoadingUnitId.Value);
                            }
                            else
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitOtherBay, requestingBay);
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
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, requestingBay);
                    }
                    else if (unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentInCell, requestingBay);
                    }
#if CHECK_BAY_SENSOR
                    else if (!this.SensorsProvider.IsLoadingUnitInLocation(messageData.Source))
                    {
                        unitToMove = null;
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, requestingBay);
                    }
#endif
                    else if (this.BaysDataProvider.GetByLoadingUnitLocation(messageData.Source).Shutter.Type != ShutterType.NotSpecified
                        && messageData.InsertLoadingUnit
                        && this.SensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed)
                    {
                        unitToMove = null;
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
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
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceDb, requestingBay);
            }

            return unitToMove != null;
        }

        #endregion
    }
}
