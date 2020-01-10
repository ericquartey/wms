namespace Ferretto.VW.MAS.DataModels
{
    public sealed class RepeatedTestProcedure : SetupProcedure
    {
        #region Fields

        private bool inProgress;

        private int performedCycles;

        private int requiredCycles;

        #endregion

        #region Properties

        public bool InProgress
        {
            get => this.inProgress;
            set => this.inProgress = value;
        }

        public int PerformedCycles
        {
            get => this.performedCycles;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.performedCycles = value;
            }
        }

        public int RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.requiredCycles = value;
            }
        }

        #endregion
    }
}
