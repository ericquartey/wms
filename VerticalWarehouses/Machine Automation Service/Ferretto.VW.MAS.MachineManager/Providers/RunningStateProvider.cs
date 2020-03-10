using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal sealed class RunningStateProvider : BaseProvider, IRunningStateProvider
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public RunningStateProvider(
            IMachineVolatileDataProvider machineProvider,
            IEventAggregator eventAggregator,
            IErrorsProvider errorsProvider)
            : base(eventAggregator)
        {
            this.machineVolatileDataProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Properties

        public Dictionary<BayNumber, bool> IsBayHoming => this.machineVolatileDataProvider.IsBayHomingExecuted;

        public MachinePowerState MachinePowerState => this.machineVolatileDataProvider.MachinePowerState;

        #endregion

        #region Methods

        public void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender)
        {
            // TODO check this call...
            this.SendCommandToMachineManager(
                new ChangeRunningStateMessageData(requestedState, null, CommandAction.Start, requestedState ? StopRequestReason.NoReason : StopRequestReason.Stop),
                $"Bay {requestingBay} requested setting Running State to {requestedState}",
                sender,
                MessageType.ChangeRunningState,
                requestingBay);
        }

        public void Stop(BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new ChangeRunningStateMessageData(
                    false,
                    null,
                    CommandAction.Stop),
                $"Bay {requestingBay} requested to stop Change Running State",
                sender,
                MessageType.ChangeRunningState,
                requestingBay);
        }

        #endregion
    }
}
