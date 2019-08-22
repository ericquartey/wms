using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public class MachineModeChangedEventArgs
    {
        #region Constructors

        public MachineModeChangedEventArgs(MachineMode machineMode, MachinePowerState machinePower)
        {
            this.MachineMode = machineMode;
            this.MachinePower = machinePower;
        }

        #endregion

        #region Properties

        public MachineMode MachineMode { get; }

        public MachinePowerState MachinePower { get; }

        #endregion
    }
}
