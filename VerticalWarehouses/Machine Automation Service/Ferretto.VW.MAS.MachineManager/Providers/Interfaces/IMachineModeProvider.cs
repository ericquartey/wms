using System.Collections.Generic;

namespace Ferretto.VW.MAS.MachineManager
{
    public interface IMachineModeProvider
    {
        #region Methods

        CommonUtils.Messages.MachineMode GetCurrent();

        void RequestChange(CommonUtils.Messages.MachineMode machineMode, List<int> loadUnits = null, int? cycles = null);

        #endregion
    }
}
