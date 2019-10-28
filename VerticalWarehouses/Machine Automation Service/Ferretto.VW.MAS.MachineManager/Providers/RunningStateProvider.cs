using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class RunningStateProvider : BaseProvider, IRunningStateProvider
    {
        #region Fields

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public RunningStateProvider(
            ISensorsProvider sensorsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
        }

        #endregion

        #region Properties

        public bool IsRunning => this.sensorsProvider.IsMachineRunning;

        #endregion

        #region Methods

        public void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender)
        {
            // TODO check this call...
            this.SendCommandToMissionManager(
                new ChangeRunningStateMessageData(requestedState, null, CommandAction.Start, requestedState ? StopRequestReason.NoReason : StopRequestReason.Stop),
                $"Bay {requestingBay} requested setting Running State to {requestedState}",
                sender,
                MessageType.ChangeRunningState,
                requestingBay);
        }

        public void Stop(BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
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
