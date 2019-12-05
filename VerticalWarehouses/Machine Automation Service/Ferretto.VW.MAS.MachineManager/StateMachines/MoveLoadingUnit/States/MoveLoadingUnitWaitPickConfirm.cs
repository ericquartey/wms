using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitWaitPickConfirm : StateBase, IMoveLoadingUnitWaitPickConfirm
    {
        #region Fields

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ISensorsProvider sensorsProvider;

        private LoadingUnitLocation ejectBayLocation;

        private BayNumber ejectBay;

        private Mission mission;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        #endregion

        #region Constructors

        public MoveLoadingUnitWaitPickConfirm(
            IBaysDataProvider baysDataProvider,
            IMissionsDataProvider missionsDataProvider,
            ICellsProvider cellsProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMachineProvider machineProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
                this.ejectBayLocation = moveData.LoadingUnitDestination;
                var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitDestination);
                this.ejectBay = bay.Number;

                this.loadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(bay.Number, this.mission.WmsId.Value);

                if (moveData.LoadingUnitId > 0)
                {
                    var machine = this.machineProvider.Get();
                    this.loadingUnitsDataProvider.SetHeight(moveData.LoadingUnitId, machine.LoadUnitMaxHeight);
                }
                this.mission.FsmStateName = this.GetType().Name;
                this.missionsDataProvider.Update(this.mission);
            }
        }

        protected override IState OnResume(CommandMessage commandMessage)
        {
            IState returnValue = this;
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.ejectBay))
#endif
            {
                if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.ejectBayLocation);
                    if (messageData.MissionType == MissionType.NoType)
                    {
                        // Remove LoadUnit

                        var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                        this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                        this.baysDataProvider.RemoveLoadingUnit(lu);

                        returnValue = this.GetState<IMoveLoadingUnitEndState>();

                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
                    }
                    else
                    {
                        // Update mission and start moving
                        this.mission.MissionType = messageData.MissionType;
                        this.mission.WmsId = messageData.WmsId;
                        this.mission.LoadingUnitSource = bayPosition.Location;
                        if (messageData.Destination == LoadingUnitLocation.Cell)
                        {
                            // prepare for finding a new empty cell
                            this.mission.DestinationCellId = null;
                            this.mission.LoadingUnitDestination = LoadingUnitLocation.Cell;
                            returnValue = this.GetState<IMoveLoadingUnitStartState>();
                        }
                        else if (bayPosition.Location != messageData.Destination)
                        {
                            // bay to bay movement
                            this.mission.LoadingUnitDestination = messageData.Destination;
                            returnValue = this.GetState<IMoveLoadingUnitStartState>();
                        }
                    }
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.requestingBay);
            }
#endif
            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            IState returnValue;
            if (reason == StopRequestReason.Error
                && this.mission.IsRestoringType()
                )
            {
                this.mission.FsmRestoreStateName = this.mission.FsmStateName;
                returnValue = this.GetState<IMoveLoadingUnitErrorState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            if (returnValue is IEndState endState)
            {
                endState.StopRequestReason = reason;
            }

            return returnValue;
        }

        #endregion
    }
}
