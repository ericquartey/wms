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
    internal class MoveLoadingUnitWaitEjectConfirm : StateBase, IMoveLoadingUnitWaitEjectConfirm
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineProvider machineProvider;

        private LoadingUnitLocation ejectBay;

        private Mission mission;

        private BayNumber requestingBay;

        #endregion

        #region Constructors

        public MoveLoadingUnitWaitEjectConfirm(
            IBaysDataProvider baysDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineProvider machineProvider,
            IMissionsDataProvider missionsDataProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"MoveLoadingUnitWaitEjectConfirm: received command {commandMessage.Type}, {commandMessage.Description}");

            this.requestingBay = commandMessage.RequestingBay;

            if (machineData is Mission moveData)
            {
                this.ejectBay = moveData.LoadingUnitDestination;

                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitWaitEjectConfirm);
                this.missionsDataProvider.Update(this.mission);

                if (moveData.LoadingUnitId > 0)
                {
                    this.loadingUnitsDataProvider.SetHeight(moveData.LoadingUnitId, 0);
                }
                moveData.Status = MissionStatus.Waiting;
                this.mission.RestoreConditions = false;
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
                var bayPosition = this.baysDataProvider.GetPositionByLocation(this.ejectBay);

                var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                this.baysDataProvider.RemoveLoadingUnit(lu);

                returnValue = this.GetState<IMoveLoadingUnitEndState>();

                ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
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
                && this.mission != null
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
