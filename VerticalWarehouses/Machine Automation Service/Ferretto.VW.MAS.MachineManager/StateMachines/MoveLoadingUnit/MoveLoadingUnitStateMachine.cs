using System;
using System.Linq;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitStateMachine : FiniteStateMachine<IMoveLoadingUnitStartState, IMoveLoadingUnitErrorState>, IMoveLoadingUnitStateMachine
    {
        #region Fields

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MoveLoadingUnitStateMachine(
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
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineModeDataProvider = machineModeDataProvider ?? throw new ArgumentNullException(nameof(machineModeDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));

            this.MachineData = new MoveLoadingUnitMachineData(this.InstanceId);
        }

        #endregion

        #region Methods

        public override bool AllowMultipleInstances(CommandMessage command)
        {
            return true;
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.MoveLoadingUnit;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return this.loadingUnitMovementProvider.FilterNotifications(notification, MessageActor.MachineManager);
        }

        protected override IState OnCommandReceived(CommandMessage commandMessage)
        {
            var newState = base.OnCommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.CommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            var newState = base.OnNotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.NotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        protected override bool OnStart(CommandMessage commandMessage, CancellationToken cancellationToken)
        {
            var returnValue = this.CheckStartConditions(commandMessage);
            if (returnValue
                && this.MachineData is Mission mission
                )
            {
                this.missionsDataProvider.Update(mission);
            }

            return returnValue;
        }

        // it not only checks, but also updates machine data
        private bool CheckStartConditions(CommandMessage commandMessage)
        {
            bool returnValue;

            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                returnValue = this.IsMachineOk(messageData);

                if (returnValue)
                {
                    returnValue = this.IsSourceOk(messageData, commandMessage.RequestingBay);
                }

                if (returnValue)
                {
                    returnValue = this.IsDestinationOk(messageData, commandMessage.RequestingBay);
                }

                if (returnValue)
                {
                    returnValue = this.IsElevatorOk();
                }
            }
            else
            {
                var description = $"Attempting to start {this.GetType()} Finite state machine with wrong ({commandMessage.Data.GetType()}) message data";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }

            return returnValue;
        }

        private bool IsDestinationOk(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay)
        {
            var machineData = (IMoveLoadingUnitMachineData)this.MachineData;
            bool returnValue = false;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.DestinationCellId != null)
                    {
                        var destinationCell = this.cellsProvider.GetById(messageData.DestinationCellId.Value);
                        returnValue = destinationCell.LoadingUnit == null && destinationCell.Status == CellStatus.Free;
                        machineData.DestinationCellId = messageData.DestinationCellId;
                        machineData.LoadingUnitDestination = LoadingUnitLocation.Cell;
                    }
                    else if (messageData.LoadingUnitId.HasValue)
                    {
                        try
                        {
                            machineData.DestinationCellId = this.cellsProvider.FindEmptyCell(messageData.LoadingUnitId.Value);
                        }
                        catch (Exception)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull);
                            throw new StateMachineException(ErrorDescriptions.WarehouseIsFull, null, MessageActor.MachineManager);
                        }
                        returnValue = true;
                        machineData.LoadingUnitDestination = LoadingUnitLocation.Cell;
                    }

                    if (!returnValue)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitDestinationCell);
                    }

                    break;

                case LoadingUnitLocation.Elevator:
                    machineData.LoadingUnitDestination = LoadingUnitLocation.Elevator;
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
                        returnValue = this.CheckBayDestination(messageData, requestingBay, bay.Positions.First().Location, machineData);
                    }
                    else
                    {
                        var destination = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
                        if (destination is LoadingUnitLocation.NoLocation)
                        {
                            throw new StateMachineException($"Upper position not defined for bay {requestingBay}", null, MessageActor.MachineManager);
                        }
                        returnValue = this.CheckBayDestination(messageData, requestingBay, destination, machineData, false);
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
                                returnValue = this.CheckBayDestination(messageData, requestingBay, destination, machineData);
                            }
                        }
                    }
                    break;

                default:
                    returnValue = this.CheckBayDestination(messageData, requestingBay, messageData.Destination, machineData);

                    break;
            }

            return returnValue;
        }

        private bool CheckBayDestination(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay, LoadingUnitLocation destination, IMoveLoadingUnitMachineData machineData, bool showErrors = true)
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
                machineData.LoadingUnitDestination = destination;
            }

            return returnValue;
        }

        private bool IsElevatorOk()
        {
            var machineData = (IMoveLoadingUnitMachineData)this.MachineData;
            var returnValue = false;

            if (machineData.LoadingUnitSource == LoadingUnitLocation.Elevator)
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

        private bool IsSourceOk(IMoveLoadingUnitMessageData messageData, BayNumber requestingBay)
        {
            var machineData = (IMoveLoadingUnitMachineData)this.MachineData;

            LoadingUnit unitToMove = null;

            switch (messageData.Source)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.SourceCellId != null)
                    {
                        var sourceCell = this.cellsProvider.GetById(messageData.SourceCellId.Value);
                        unitToMove = sourceCell.LoadingUnit;

                        machineData.LoadingUnitSource = LoadingUnitLocation.Cell;
                        machineData.LoadingUnitCellSourceId = sourceCell.Id;
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
                            machineData.LoadingUnitSource = LoadingUnitLocation.Cell;
                            machineData.LoadingUnitCellSourceId = sourceCell.Id;
                        }
                        else
                        {
                            var sourceBay = this.baysDataProvider.GetLoadingUnitLocationByLoadingUnit(unitToMove.Id);

                            if (sourceBay != LoadingUnitLocation.NoLocation)
                            {
                                machineData.LoadingUnitSource = sourceBay;
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
                                    machineData.LoadingUnitSource = LoadingUnitLocation.Elevator;
                                }
                            }
                        }

                        if (machineData.LoadingUnitSource == LoadingUnitLocation.NoLocation)
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

                    machineData.LoadingUnitSource = messageData.Source;
                    break;
            }

            if (unitToMove != null)
            {
                machineData.LoadingUnitId = unitToMove.Id;
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
