using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionsManager.Providers.Interfaces
{
    public interface IRunningStateProvider
    {
        #region Properties

        bool IsRunning { get; }

        #endregion

        #region Methods

        void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
