using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineVolatileDataProvider
    {
        #region Properties

        BayNumber BayTestNumber { get; set; }

        int CyclesTested { get; set; }

        int? CyclesToTest { get; set; }

        double ElevatorHorizontalPosition { get; set; }

        double ElevatorVerticalPosition { get; set; }

        Dictionary<BayNumber, bool> IsBayHomingExecuted { get; set; }

        Dictionary<BayNumber, bool> IsBayLightOn { get; }

        bool IsHomingActive { get; set; }

        bool IsHomingExecuted { get; set; }

        bool IsMachineRunning { get; }

        bool? IsOneTonMachine { get; set; }

        List<int> LoadUnitsToTest { get; set; }

        MachinePowerState MachinePowerState { get; set; }

        MachineMode Mode { get; set; }

        #endregion

        #region Methods

        double GetBayEncoderPosition(BayNumber bayNumber);

        void SetBayEncoderPosition(BayNumber bayNumber, double position);

        #endregion
    }
}
