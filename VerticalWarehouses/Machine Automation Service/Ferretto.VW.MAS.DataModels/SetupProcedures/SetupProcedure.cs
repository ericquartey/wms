namespace Ferretto.VW.MAS.DataModels
{
    public class SetupProcedure : DataModel
    {
        #region Fields

        private double feedRate = 1;

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

        public bool? InProgress { get; set; }

        public bool IsBypassed { get; set; }

        public bool IsCompleted { get; set; }

        #endregion
    }
}
