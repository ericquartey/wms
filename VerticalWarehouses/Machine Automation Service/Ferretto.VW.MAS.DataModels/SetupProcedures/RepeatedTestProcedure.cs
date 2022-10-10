namespace Ferretto.VW.MAS.DataModels
{
    public sealed class RepeatedTestProcedure : SetupProcedure
    {
        #region Fields

        private int performedCycles;

        private int requiredCycles;

        #endregion

        #region Properties

        public int PerformedCycles
        {
            get => this.performedCycles;
            set
            {
                this.performedCycles = value;
            }
        }

        public int RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                this.requiredCycles = value;
            }
        }

        #endregion
    }
}
