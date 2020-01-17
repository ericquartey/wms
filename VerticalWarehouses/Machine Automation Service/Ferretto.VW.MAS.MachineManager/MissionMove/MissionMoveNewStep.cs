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
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
                }
                returnValue = false;
            }
            if (returnValue
                && destination != LoadingUnitLocation.NoLocation)
            {
                var destinationBay = this.BaysDataProvider.GetByLoadingUnitLocation(destination);
                if (destinationBay != null)
                {
                    if (destinationBay.Number != requestingBay)
                    {
                        // move from bay to bay
                        if (destinationBay.Shutter.Type != ShutterType.NotSpecified
                            && this.SensorsProvider.GetShutterPosition(destinationBay.Shutter.Inverter.Index) != ShutterPosition.Closed
                            && this.SensorsProvider.GetShutterPosition(destinationBay.Shutter.Inverter.Index) != ShutterPosition.Opened
                            )
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, requestingBay);
                            }
                            returnValue = false;
                        }
                    }
                    else
                    {
                        if (!this.CheckBayHeight(destinationBay, destination, mission))
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
            if (returnValue)
            {
                mission.LoadUnitDestination = destination;
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
                        var destinationCell = this.CellsProvider.GetById(messageData.DestinationCellId.Value);
                        returnValue = (destinationCell.LoadingUnit == null && destinationCell.IsFree);
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
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.WarehouseIsFull, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            return false;
                        }
                        returnValue = true;
                        mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                    }

                    if (!returnValue && showErrors)
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
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, mission, showErrors);
                    }
                    else
                    {
                        var destination = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (destination is LoadingUnitLocation.NoLocation)
                        {
                            if (showErrors)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        returnValue = this.CheckBayDestination(messageData, requestingBay, destination, mission, false);
                        if (!returnValue)
                        {
                            // if upper position is not empty we can try lower position
                            destination = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                            if (destination is LoadingUnitLocation.NoLocation)
                            {
                                if (showErrors)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedBottom, this.Mission.TargetBay, MessageActor.MachineManager);
                                }
                                else
                                {
                                    return false;
                                }
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
                                returnValue = this.CheckBayDestination(messageData, requestingBay, destination, mission, showErrors);
                            }
                        }
                    }
                    break;

                default:
                    returnValue = this.CheckBayDestination(messageData, requestingBay, messageData.Destination, mission, showErrors);

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

                case MissionType.FullTest:
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
                    }

                    if (unitToMove == null)
                    {
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, requestingBay);
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
                        }
                    }
                    else if (showErrors)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, requestingBay);
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
                    }
                    else if (unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        if (showErrors)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentInCell, requestingBay);
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
            }

            return unitToMove != null;
        }

        #endregion
    }
}
