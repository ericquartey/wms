namespace Ferretto.VW.MAS.DataModels
{
    public class ElevatorAxisManualParameters : DataModel
    {
        #region Fields

        private double feedRate = 0.15;

        private double feedRateAfterZero = 0.3;

        #endregion

        #region Properties

        public double FeedRate
        {
            get => this.feedRate;
            set
            {
                if (value <= 0 || value > 1)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.feedRate = value;
            }
        }

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

        public double? TargetDistance { get; set; }

        public double? TargetDistanceAfterZero { get; set; }

        #endregion
    }
}
