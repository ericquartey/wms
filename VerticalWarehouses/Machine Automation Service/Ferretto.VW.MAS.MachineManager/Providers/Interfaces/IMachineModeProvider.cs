using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager
{
    public interface IMachineModeProvider
    {
        #region Methods

        CommonUtils.Messages.MachineMode GetCurrent();

        void RequestChange(MachineMode machineMode,
                           BayNumber bayNumber = BayNumber.None,
                           List<int> loadUnits = null,
                           int? cycles = null,
                           bool randomCells = false,
                           bool optimizeRotationClass = false);

        void StopTest();

        #endregion
    }
}
