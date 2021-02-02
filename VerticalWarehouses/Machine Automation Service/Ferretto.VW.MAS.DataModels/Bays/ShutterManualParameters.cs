namespace Ferretto.VW.MAS.DataModels
{
    public class ShutterManualParameters : DataModel
    {
        #region Fields

        private double feedRate = 0.15;

        private double highSpeedDurationClose;

        private double highSpeedDurationOpen;

        private double? highSpeedHalfDurationClose;

        private double? highSpeedHalfDurationOpen;

        private double maxSpeed;

        private double minSpeed;

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

        public double HighSpeedDurationClose
        {
            get => this.highSpeedDurationClose;
            set
            {
                this.highSpeedDurationClose = value;
            }
        }

        public double HighSpeedDurationOpen
        {
            get => this.highSpeedDurationOpen;
            set
            {
                this.highSpeedDurationOpen = value;
            }
        }

        public double? HighSpeedHalfDurationClose
        {
            get => this.highSpeedHalfDurationClose;
            set
            {
                this.highSpeedHalfDurationClose = value;
            }
        }

        public double? HighSpeedHalfDurationOpen
        {
            get => this.highSpeedHalfDurationOpen;
            set
            {
                this.highSpeedHalfDurationOpen = value;
            }
        }

        #endregion
    }
}
