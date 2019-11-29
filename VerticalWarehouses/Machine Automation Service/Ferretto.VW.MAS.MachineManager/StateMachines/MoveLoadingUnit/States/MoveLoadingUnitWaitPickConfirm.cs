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

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ISensorsProvider sensorsProvider;

        private LoadingUnitLocation ejectBay;

        private BayNumber requestingBay;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitWaitPickConfirm(
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            this.requestingBay = commandMessage.RequestingBay;

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
            }
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                this.ejectBay = messageData.Destination;
            }
            this.loadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(commandMessage.RequestingBay, this.mission.WmsId.Value);

            ((IMoveLoadingUnitMachineData)machineData).FsmStateName = this.GetType().Name;
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
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.ejectBay);
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
                            this.mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.mission.LoadingUnitId);
                        }
                        else
                        {
                            // TODO bay to bay movement
                        }
                        returnValue = this.GetState<IMoveLoadingUnitStartState>();
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
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
