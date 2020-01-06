using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MissionMoveProvider : IMissionMoveProvider
    {
        #region Fields

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly NotificationEvent notificationEvent;

        public ILogger<MachineManagerService> Logger { get; }

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            ICellsProvider cellsProvider,
            ISensorsProvider sensorsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IErrorsProvider errorsProvider,
            IMachineModeVolatileDataProvider machineModeDataProvider,
            IMissionsDataProvider missionsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<MachineManagerService> logger
            )
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineModeDataProvider = machineModeDataProvider ?? throw new ArgumentNullException(nameof(machineModeDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Methods

        public bool TryCreateMachineMission(CommandMessage command, out int? missionId)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            missionId = null;

            if (command.Data is IMoveLoadingUnitMessageData messageData
                && messageData.LoadingUnitId.HasValue
                )
            {
                if (this.missionsDataProvider.CanCreateMission(messageData.LoadingUnitId.Value, command.RequestingBay))
                {
                    // if there is a mission waiting we have to take her place
                    var waitMission = this.missionsDataProvider.GetAllExecutingMissions().FirstOrDefault(m =>
                        m.LoadingUnitId == messageData.LoadingUnitId.Value
                        && m.Status == MissionStatus.Waiting
                        );
                    if (waitMission != null)
                    {
                        try
                        {
                            this.missionsDataProvider.Delete(waitMission.Id);
                            this.Logger.LogDebug($"{this.GetType().Name}: Delete {waitMission}");
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }

                    var newMission = this.missionsDataProvider.CreateBayMission(messageData.LoadingUnitId.Value, command.RequestingBay);
                    missionId = newMission.Id;
                    return true;
                }
            }

            return false;
        }

        public bool Start(int missionId, CommandMessage commandMessage)
        {
            var mission = this.missionsDataProvider.GetById(missionId);
            var returnValue = this.CheckStartConditions(mission, commandMessage);
            if (returnValue)
            {
                this.missionsDataProvider.Update(mission);
                this.Logger.LogDebug($"{this.GetType().Name}: {mission}");
            }

            return returnValue;
        }

        // it checks message data and updates mission
        private bool CheckStartConditions(Mission mission, CommandMessage commandMessage)
        {
            bool returnValue;

            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
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
                var description = $"Attempting to start {this.GetType()} Finite state machine with wrong ({commandMessage.Data.GetType()}) message data";

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
                        var destinationCell = this.cellsProvider.GetById(messageData.DestinationCellId.Value);
                        returnValue = (destinationCell.LoadingUnit == null && destinationCell.IsFree);
                        mission.DestinationCellId = messageData.DestinationCellId;
                        mission.LoadingUnitDestination = LoadingUnitLocation.Cell;
                    }
                    else if (messageData.LoadingUnitId.HasValue)
                    {
                        try
                        {
                            mission.DestinationCellId = this.cellsProvider.FindEmptyCell(messageData.LoadingUnitId.Value);
                        }
                        catch (Exception)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull);
                            throw new StateMachineException(ErrorDescriptions.WarehouseIsFull, null, MessageActor.MachineManager);
                        }
                        returnValue = true;
                        mission.LoadingUnitDestination = LoadingUnitLocation.Cell;
                    }

                    if (!returnValue)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitDestinationCell);
                    }

                    break;

                case LoadingUnitLocation.Elevator:
                    mission.LoadingUnitDestination = LoadingUnitLocation.Elevator;
                    returnValue = true;
                    break;

                case LoadingUnitLocation.LoadingUnit:
                    var description = $"Attempting to start {this.GetType()} Finite state machine with Loading Unit as destination Type";

                    throw new StateMachineException(description, null, MessageActor.MachineManager);

                case LoadingUnitLocation.NoLocation:
                    // destination is bay, but first we must decide which position to use
                    var bay = this.baysDataProvider.GetByNumber(requestingBay);
                    if (bay.Positions.Count() == 1)
                    {
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, mission);
                    }
                    else
                    {
                        var destination = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (destination is LoadingUnitLocation.NoLocation)
                        {
                            throw new StateMachineException($"Upper position not defined for bay {requestingBay}", null, MessageActor.MachineManager);
                        }
                        returnValue = this.CheckBayDestination(messageData, requestingBay, destination, mission, false);
                        if (!returnValue)
                        {
                            // if upper position is not empty we can try lower position
                            destination = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                            if (destination is LoadingUnitLocation.NoLocation)
                            {
                                throw new StateMachineException($"Lower position not defined for bay {requestingBay}", null, MessageActor.MachineManager);
                            }
                            // the other mission must be in waiting state
                            var activeMission = this.missionsDataProvider.GetAllActiveMissionsByBay(requestingBay)
                                .FirstOrDefault(x => (x.Status == MissionStatus.Executing || x.Status == MissionStatus.Waiting)
                                    && x.NeedHomingAxis != Axis.BayChain);

                            if (activeMission != null
                                && activeMission.Status != MissionStatus.Waiting
                                )
                            {
                                this.Logger.LogTrace($"IsDestinationOk: waiting for mission in upper position {activeMission.Id}, LoadUnit {activeMission.LoadingUnitId}; bay {requestingBay}");
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

        private bool CheckBayDestination(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, LoadingUnitLocation destination, Mission mission, bool showErrors = true)
        {
            bool returnValue;
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(destination))
#endif
            {
                returnValue = (messageData.Source == destination)
                    || (this.baysDataProvider.GetLoadingUnitByDestination(destination) == null);
            }
            if (!returnValue)
            {
                if (showErrors)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitDestinationBay);
                }
            }
            else if (this.baysDataProvider.GetByLoadingUnitLocation(destination).Shutter.Type != ShutterType.NotSpecified
                && this.sensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed
                && this.sensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Opened
                )
            {
                if (showErrors)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterOpen);
                }
                returnValue = false;
            }
            if (returnValue
                && destination != LoadingUnitLocation.NoLocation)
            {
                var destinationBay = this.baysDataProvider.GetByLoadingUnitLocation(destination);
                if (destinationBay != null
                    && destinationBay.Number != requestingBay
                    )
                {
                    // move from bay to bay
                    if (this.baysDataProvider.GetByLoadingUnitLocation(destination).Shutter.Type != ShutterType.NotSpecified
                        && this.sensorsProvider.GetShutterPosition(destinationBay.Number) != ShutterPosition.Closed
                        && this.sensorsProvider.GetShutterPosition(destinationBay.Number) != ShutterPosition.Opened
                        )
                    {
                        if (showErrors)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterOpen);
                        }
                        returnValue = false;
                    }
                }
            }
            if (returnValue)
            {
                mission.LoadingUnitDestination = destination;
            }

            return returnValue;
        }

        private bool IsElevatorOk(Mission mission)
        {
            var returnValue = false;

            if (mission.LoadingUnitSource == LoadingUnitLocation.Elevator)
            {
                returnValue = this.elevatorDataProvider.GetLoadingUnitOnBoard() != null;
                if (!returnValue)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceElevator);
                }
                else
                {
                    returnValue = this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator);
                    }
                }
            }
            else
            {
                returnValue = this.elevatorDataProvider.GetLoadingUnitOnBoard() == null;

                if (!returnValue)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitElevator);
                }
                else
                {
                    returnValue = !this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
                    if (!returnValue)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator);
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
                    returnValue = (this.machineModeDataProvider.Mode == MachineMode.Manual);
                    break;

                case MissionType.Compact:
                    returnValue = (this.machineModeDataProvider.Mode == MachineMode.Compact);
                    break;

                case MissionType.WMS:
                    returnValue = (this.machineModeDataProvider.Mode == MachineMode.Automatic);
                    break;

                case MissionType.Test:
                    returnValue = (this.machineModeDataProvider.Mode == MachineMode.Test);
                    break;

                default:
                    var description = $"Attempting to start {this.GetType()} Finite state machine with invalid MissionType {messageData.MissionType}";

                    throw new StateMachineException(description, null, MessageActor.MachineManager);
            }
            if (!returnValue)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineModeNotValid);
                return false;
            }

#if !CHECK_BAY_SENSOR
            return true;
#else
            else
            {
                returnValue = this.sensorsProvider.IsLoadingUnitInLocation(messageData.Source);
            }
            if (!returnValue)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorNoLoadingUnitInSource);
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
                        var sourceCell = this.cellsProvider.GetById(messageData.SourceCellId.Value);
                        unitToMove = sourceCell.LoadingUnit;

                        mission.LoadingUnitSource = LoadingUnitLocation.Cell;
                        mission.LoadingUnitCellSourceId = sourceCell.Id;
                    }

                    if (unitToMove == null)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceCell);
                        return false;
                    }

                    break;

                case LoadingUnitLocation.LoadingUnit:
                    if (messageData.LoadingUnitId != null)
                    {
                        unitToMove = this.loadingUnitsDataProvider.GetById(messageData.LoadingUnitId.Value);
                    }

                    if (unitToMove != null)
                    {
                        var sourceCell = this.cellsProvider.GetByLoadingUnitId(unitToMove.Id);

                        if (sourceCell != null)
                        {
                            mission.LoadingUnitSource = LoadingUnitLocation.Cell;
                            mission.LoadingUnitCellSourceId = sourceCell.Id;
                        }
                        else
                        {
                            var sourceBay = this.baysDataProvider.GetLoadingUnitLocationByLoadingUnit(unitToMove.Id);

                            if (sourceBay != LoadingUnitLocation.NoLocation)
                            {
                                mission.LoadingUnitSource = sourceBay;
                            }
                            else
                            {
                                // it is not in bay and not in cell, maybe it is on the elevator?
                                var elevatorLoadUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
                                if (elevatorLoadUnit is null
                                    || unitToMove.Id != elevatorLoadUnit.Id)
                                {
                                    unitToMove = null;
                                    this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotLoaded);
                                }
                                else
                                {
                                    mission.LoadingUnitSource = LoadingUnitLocation.Elevator;
                                }
                            }
                        }

                        if (mission.LoadingUnitSource == LoadingUnitLocation.NoLocation)
                        {
                            unitToMove = null;
                            this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotLoaded);
                        }
                    }
                    else
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotFound);
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
                            var bayPosition = this.baysDataProvider.GetLoadingUnitLocationByLoadingUnit(messageData.LoadingUnitId.Value);
                            if (bayPosition == LoadingUnitLocation.NoLocation
                                || bayPosition == messageData.Source
                                )
                            {
                                unitToMove = this.loadingUnitsDataProvider.GetById(messageData.LoadingUnitId.Value);
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitOtherBay);
                            }
                        }
                    }
                    else
                    {
                        unitToMove = this.baysDataProvider.GetLoadingUnitByDestination(messageData.Source);
                    }

                    if (unitToMove == null)
                    {
                        unitToMove = null;
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotFound);
                    }
                    else if (unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitPresentInCell);
                    }
#if CHECK_BAY_SENSOR
                    else if (!this.sensorsProvider.IsLoadingUnitInLocation(messageData.Source))
                    {
                        unitToMove = null;
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceBay);
                    }
#endif
                    else if (this.baysDataProvider.GetByLoadingUnitLocation(messageData.Source).Shutter.Type != ShutterType.NotSpecified
                        && messageData.InsertLoadingUnit
                        && this.sensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed)
                    {
                        unitToMove = null;
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterOpen);
                    }
                    if (unitToMove == null)
                    {
                        return false;
                    }

                    mission.LoadingUnitSource = messageData.Source;
                    break;
            }

            if (unitToMove != null)
            {
                mission.LoadingUnitId = unitToMove.Id;
            }
            else
            {
                unitToMove = null;
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceDb);
            }

            return unitToMove != null;
        }

        #endregion
    }
}
