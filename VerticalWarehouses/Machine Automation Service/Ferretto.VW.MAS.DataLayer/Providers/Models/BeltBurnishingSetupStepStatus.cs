namespace Ferretto.VW.MAS.DataLayer
{
    public class BeltBurnishingSetupStepStatus : SetupStepStatus
    {
        #region Properties

        public static new BeltBurnishingSetupStepStatus Complete => new BeltBurnishingSetupStepStatus { CanBePerformed = true, IsCompleted = true };

        public int CompletedCycles { get; set; }

        #endregion
    }
}
