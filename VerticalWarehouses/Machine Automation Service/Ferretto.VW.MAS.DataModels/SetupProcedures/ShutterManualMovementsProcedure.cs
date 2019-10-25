namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ShutterManualMovementsProcedure : SetupProcedure
    {
        #region Fields

        private double highSpeedDurationClose;

        private double highSpeedDurationOpen;

        private double maxSpeed;

        private double minSpeed;

        #endregion

        #region Properties

        public double HighSpeedDurationClose
        {
            get => this.highSpeedDurationClose;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.highSpeedDurationClose = value;
            }
        }

        public double HighSpeedDurationOpen
        {
            get => this.highSpeedDurationOpen;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.highSpeedDurationOpen = value;
            }
        }

        public double MaxSpeed
        {
            get => this.maxSpeed;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.maxSpeed = value;
            }
        }

        public double MinSpeed
        {
            get => this.minSpeed;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.minSpeed = value;
            }
        }

        #endregion
    }
}
