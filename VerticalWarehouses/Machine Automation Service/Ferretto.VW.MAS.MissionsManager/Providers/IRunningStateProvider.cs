using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    public interface IRunningStateProvider
    {


        #region Methods

        void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
