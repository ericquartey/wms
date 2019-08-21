namespace Ferretto.VW.MAS.DataLayer.Providers.Models
{
    public class SetupStepStatus
    {
        #region Properties

        public static SetupStepStatus Complete => new SetupStepStatus { CanBePerformed = true, IsCompleted = true };

        public bool CanBePerformed { get; set; }

        public bool IsCompleted { get; set; }

        #endregion
    }
}
