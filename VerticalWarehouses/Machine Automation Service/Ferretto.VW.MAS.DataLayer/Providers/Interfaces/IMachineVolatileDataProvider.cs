using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineVolatileDataProvider
    {
        #region Properties

        MachineMode Mode { get; set; }

        Dictionary<BayNumber, bool> IsBayLightOn { get; }

        bool IsHomingExecuted { get; set; }

        bool IsMachineRunning { get; set; }

        double ElevatorHorizontalPosition { get; set; }

        double ElevatorVerticalPosition { get; set; }
        Dictionary<BayNumber, bool> IsBayHomingExecuted { get; set; }

        #endregion

        #region Methods

        double GetBayEncoderPosition(BayNumber bayNumber);

        void SetBayEncoderPosition(BayNumber bayNumber, double position);

        #endregion
    }
}
