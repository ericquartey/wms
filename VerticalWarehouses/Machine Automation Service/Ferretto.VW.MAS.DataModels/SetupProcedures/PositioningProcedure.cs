namespace Ferretto.VW.MAS.DataModels
{
    public class PositioningProcedure : SetupProcedure
    {
        #region Fields

        private bool inProgress;

        private double step;

        #endregion

        #region Properties

        public bool InProgress
        {
            get => this.inProgress;
            set => this.inProgress = value;
        }

        public double Step
        {
            get => this.step;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.step = value;
            }
        }

        #endregion
    }
}
