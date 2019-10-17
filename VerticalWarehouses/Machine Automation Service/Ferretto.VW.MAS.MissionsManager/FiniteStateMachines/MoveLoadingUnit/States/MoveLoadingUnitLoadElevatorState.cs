using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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
    internal class MoveLoadingUnitLoadElevatorState : StateBase, IMoveLoadingUnitLoadElevatorState
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private IMoveLoadingUnitMachineData moveData;

        #endregion

        #region Constructors

        public MoveLoadingUnitLoadElevatorState(
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
            if (machineData is IMoveLoadingUnitMachineData machineMoveData)
            {
                this.moveData = machineMoveData;

                var direction = HorizontalMovementDirection.Backwards;
                switch (this.moveData.LoadingUnitSource)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.moveData.LoadingUnitCellSourceId != null)
                        {
                            var cell = this.cellsProvider.GetCellById(this.moveData.LoadingUnitCellSourceId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }

                        break;

                    default:
                        var bay = this.baysProvider.GetByLoadingUnitLocation(this.moveData.LoadingUnitSource);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;

                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, true, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
            else
            {
                var description = $"Move Loading Unit Load Unit Sate received wrong initialization data ({commandMessage.Data.GetType()})";

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
                        this.elevatorDataProvider.LoadLoadingUnit(this.moveData.LoadingUnitId);

                        if (this.moveData.LoadingUnitSource == LoadingUnitLocation.Cell)
                        {
                            var moveDataLoadingUnitCellSourceId = this.moveData.LoadingUnitCellSourceId;

                            if (moveDataLoadingUnitCellSourceId != null)
                            {
                                this.cellsProvider.UnloadLoadingUnit(moveDataLoadingUnitCellSourceId.Value);
                            }
                        }
                        else
                        {
                            this.baysProvider.UnloadLoadingUnit(this.moveData.LoadingUnitSource);
                        }

                        transaction.Commit();
                    }

                    returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();

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
