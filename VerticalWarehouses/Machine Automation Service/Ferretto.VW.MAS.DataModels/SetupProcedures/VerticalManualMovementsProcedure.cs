namespace Ferretto.VW.MAS.DataModels
{
    public sealed class VerticalManualMovementsProcedure : SetupProcedure
    {
        #region Fields

        private double feedRateAfterZero;

        #endregion

        #region Properties

        public double FeedRateAfterZero
        {
            get => this.feedRateAfterZero;
            set
            {
                if (value <= 0 || value > 1)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.feedRateAfterZero = value;
            }
        }

        public double NegativeTargetDirection { get; set; }

        public double PositiveTargetDirection { get; set; }

        #endregion
    }
}
