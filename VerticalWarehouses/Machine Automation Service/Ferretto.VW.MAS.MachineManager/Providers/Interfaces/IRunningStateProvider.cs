using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IRunningStateProvider
    {
        #region Properties

        bool IsHoming { get; }

        MachinePowerState MachinePowerState { get; }

        #endregion

        #region Methods

        void SetRunningState(bool requestedState, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
