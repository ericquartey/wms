using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitDepositUnitState : StateBase, IMoveLoadingUnitDepositUnitState
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private LoadingUnitLocation destination;

        private int? destinationCellId;

        private int loadingUnitId;

        #endregion

        #region Constructors

        public MoveLoadingUnitDepositUnitState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IElevatorDataProvider elevatorDataProvider,
            IBaysProvider baysProvider,
            ICellsProvider cellsProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData && machineData is IMoveLoadingUnitMachineData moveData)
            {
                this.loadingUnitId = moveData.LoadingUnitId;
                this.destination = messageData.Destination;
                this.destinationCellId = messageData.DestinationCellId;

                var direction = HorizontalMovementDirection.Backwards;
                switch (this.destination)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.destinationCellId != null)
                        {
                            var cell = this.cellsProvider.GetCellById(this.destinationCellId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }

                        break;

                    default:
                        var bay = this.baysProvider.GetByLoadingUnitLocation(this.destination);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;

                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, false, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
            else
            {
                var description = $"Move Loading Unit Deposit Unit Sate received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:

                    using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                    {
                        this.elevatorDataProvider.UnloadLoadingUnit();

                        if (this.destination == LoadingUnitLocation.Cell)
                        {
                            var moveDataLoadingUnitCellSourceId = this.destinationCellId;

                            if (moveDataLoadingUnitCellSourceId != null)
                            {
                                this.cellsProvider.LoadLoadingUnit(this.loadingUnitId, moveDataLoadingUnitCellSourceId.Value);
                            }
                        }
                        else
                        {
                            this.baysProvider.LoadLoadingUnit(this.loadingUnitId, this.destination);
                        }

                        transaction.Commit();
                    }

                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    break;

                case MessageStatus.OperationError:
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                    ((IEndState)returnValue).ErrorMessage = notification;
                    break;
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
