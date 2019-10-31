using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    public interface IMachineModeProvider
    {
        #region Methods

        CommonUtils.Messages.MachineMode GetCurrent();

        void RequestChange(CommonUtils.Messages.MachineMode machineMode);

        #endregion
    }
}
