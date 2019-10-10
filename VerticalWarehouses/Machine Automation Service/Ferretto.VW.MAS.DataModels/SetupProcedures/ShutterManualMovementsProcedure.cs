namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ShutterManualMovementsProcedure : SetupProcedure
    {
        #region Fields

        private double acceleration;

        private double deceleration;

        private double higherDistance;

        private double highSpeedDurationClose;

        private double highSpeedDurationOpen;

        private double lowerDistance;

        private double maxSpeed;

        private double minSpeed;

        #endregion

        #region Properties

        public double Acceleration
        {
            get => this.acceleration;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.acceleration = value;
            }
        }

        public double Deceleration
        {
            get => this.deceleration;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.deceleration = value;
            }
        }

        public double HigherDistance
        {
            get => this.higherDistance;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.higherDistance = value;
            }
        }

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

        public double LowerDistance
        {
            get => this.lowerDistance;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.lowerDistance = value;
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
