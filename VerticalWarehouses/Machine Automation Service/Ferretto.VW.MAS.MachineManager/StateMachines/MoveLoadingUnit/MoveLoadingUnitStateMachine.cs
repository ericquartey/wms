using System;
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
    internal class MoveLoadingUnitStateMachine : FiniteStateMachine<IMoveLoadingUnitStartState>, IMoveLoadingUnitStateMachine
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MoveLoadingUnitStateMachine(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ILoadingUnitsProvider loadingUnitsProvider,
            ICellsProvider cellsProvider,
            ISensorsProvider sensorsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IErrorsProvider errorsProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new ArgumentNullException(nameof(loadingUnitsProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));

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

            return returnValue;
        }

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
            bool returnValue = false;
            switch (messageData.Destination)
            {
                case LoadingUnitLocation.Cell:
                    if (messageData.DestinationCellId != null)
                    {
                        var destinationCell = this.cellsProvider.GetById(messageData.DestinationCellId.Value);
                        returnValue = destinationCell.LoadingUnit == null && destinationCell.Status == CellStatus.Free;
                    }

                    if (!returnValue)
                    {
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitDestinationCell);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitDestinationCell);
                    }

                    break;

                case LoadingUnitLocation.LoadingUnit:
                    var description = $"Attempting to start {this.GetType()} Finite state machine with Loading Unit as destination Type";

                    throw new StateMachineException(description, null, MessageActor.MachineManager);

                default:
#if CHECK_BAY_SENSOR
                    if (!this.sensorsProvider.IsLoadingUnitInLocation(messageData.Destination))
#endif
                    {
                        returnValue = this.baysDataProvider.GetLoadingUnitByDestination(messageData.Destination) == null;
                    }
                    if (!returnValue)
                    {
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitDestinationBay);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitDestinationBay);
                    }
                    else if (this.baysDataProvider.GetByLoadingUnitLocation(messageData.Destination).Shutter.Type != ShutterType.NotSpecified
                        && this.sensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed
                        )
                    {
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterOpen);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterOpen);
                        returnValue = false;
                    }

                    break;
            }

            return returnValue;
        }

        private bool IsElevatorOk()
        {
            var returnValue = this.elevatorDataProvider.GetLoadingUnitOnBoard() == null;

            if (!returnValue)
            {
                this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitElevator);
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitElevator);
            }
#if CHECK_BAY_SENSOR
            else
            {
                returnValue = !this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator);
            }
#endif
            if (!returnValue)
            {
                this.Logger.LogError(ErrorDescriptions.LoadUnitPresentOnEmptyElevator);
                this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitPresentOnEmptyElevator);
            }

            return returnValue;
        }

        private bool IsMachineOk(IMoveLoadingUnitMessageData messageData)
        {
#if !CHECK_BAY_SENSOR
            return true;
#endif
            var returnValue = this.sensorsProvider.IsLoadingUnitInLocation(messageData.Source);
            if (!returnValue)
            {
                this.Logger.LogError(ErrorDescriptions.MachineManagerErrorNoLoadingUnitInSource);
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorNoLoadingUnitInSource);
            }

            return returnValue;
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
                    }

                    break;

                case LoadingUnitLocation.LoadingUnit:
                    if (messageData.LoadingUnitId != null)
                    {
                        unitToMove = this.loadingUnitsProvider.GetById(messageData.LoadingUnitId.Value);
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

                    break;

                default:
                    if (messageData.InsertLoadingUnit)
                    {
                        if (messageData.LoadingUnitId != null)
                        {
                            unitToMove = this.loadingUnitsProvider.GetById(messageData.LoadingUnitId.Value);
                        }
                    }
                    else
                    {
                        unitToMove = this.baysDataProvider.GetLoadingUnitByDestination(messageData.Source);
                    }

                    if (unitToMove == null || unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitNotFound);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotFound);
                    }
                    else if (unitToMove.CellId.HasValue)
                    {
                        unitToMove = null;
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitSourceDb);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceDb);
                    }
#if CHECK_BAY_SENSOR
                    else if (!this.sensorsProvider.IsLoadingUnitInLocation(messageData.Source))
                    {
                        unitToMove = null;
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitSourceBay);
                        this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceBay);
                    }
#endif
                    else if (this.baysDataProvider.GetByLoadingUnitLocation(messageData.Source).Shutter.Type != ShutterType.NotSpecified
                        && messageData.InsertLoadingUnit && this.sensorsProvider.GetShutterPosition(requestingBay) != ShutterPosition.Closed
                        )
                    {
                        unitToMove = null;
                        this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterOpen);
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
                this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitSourceDb);
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitSourceDb);
            }

            return unitToMove != null;
        }

        #endregion
    }
}
