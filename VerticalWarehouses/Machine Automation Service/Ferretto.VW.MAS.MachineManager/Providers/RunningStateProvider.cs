using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal sealed class RunningStateProvider : BaseProvider, IRunningStateProvider
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public RunningStateProvider(
            IMachineProvider machineProvider,
            IEventAggregator eventAggregator,
            IErrorsProvider errorsProvider)
            : base(eventAggregator)
        {
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Properties

        public bool IsRunning => this.machineProvider.IsMachineRunning;

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
