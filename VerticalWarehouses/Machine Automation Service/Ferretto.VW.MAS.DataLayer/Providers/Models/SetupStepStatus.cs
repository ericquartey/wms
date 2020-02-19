namespace Ferretto.VW.MAS.DataLayer
{
    public class SetupStepStatus
    {
        #region Properties

        public static SetupStepStatus Complete => new SetupStepStatus { CanBePerformed = true, IsCompleted = true };

        public bool CanBePerformed { get; set; }

        public bool InProgress { get; set; }

        public bool IsBypassed { get; set; }

        public bool IsCompleted { get; set; }

        #endregion
    }
}
