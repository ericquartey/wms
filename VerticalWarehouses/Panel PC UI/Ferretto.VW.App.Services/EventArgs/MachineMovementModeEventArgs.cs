using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.App.Services.EventArgs
{
    public class MachineMovementModeEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineMovementModeEventArgs(MachineMovementMode machineMovementMode)
        {
            this.MachineMovementMode = machineMovementMode;
        }

        #endregion

        #region Properties

        public MachineMovementMode MachineMovementMode { get; }

        #endregion
    }
}
