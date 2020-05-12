using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager
{
    public interface IMachineModeProvider
    {
        #region Methods

        CommonUtils.Messages.MachineMode GetCurrent();

        void RequestChange(CommonUtils.Messages.MachineMode machineMode, BayNumber bayNumber = BayNumber.None, List<int> loadUnits = null, int? cycles = null);
        void StopTest();

        #endregion
    }
}
