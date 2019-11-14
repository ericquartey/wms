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

        private readonly IBaysDataProvider baysProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ISensorsProvider sensorsProvider;

        private LoadingUnitLocation ejectBay;

        private BayNumber requestingBay;

        #endregion

        #region Constructors

        public MoveLoadingUnitWaitEjectConfirm(
            IBaysDataProvider baysProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.requestingBay = commandMessage.RequestingBay;

            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                this.ejectBay = messageData.Destination;
            }
        }

        protected override IState OnResume()
        {
            IState returnValue = this;

            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.ejectBay))
            {
                var bayPosition = this.baysProvider.GetPositionByLocation(this.ejectBay);

                this.baysProvider.SetLoadingUnit(bayPosition.Id, null);

                returnValue = this.GetState<IMoveLoadingUnitEndState>();

                ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
            }
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.requestingBay);
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
