using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class RunningStateProvider : BaseProvider, IRunningStateProvider
    {


        #region Constructors

        public RunningStateProvider(IEventAggregator eventAggregator)
            : base(eventAggregator) { }

        #endregion



        #region Methods

        public void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new ChangeRunningStateMessageData(
                    requestedState
                    ),
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
                    CommandAction.Stop
                ),
                $"Bay {requestingBay} requested to stop Change Running State",
                sender,
                MessageType.ChangeRunningState,
                requestingBay);
        }

        #endregion
    }
}
