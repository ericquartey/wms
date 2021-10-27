namespace Ferretto.VW.MAS.DataModels
{
    public class MovementParameters : DataModel
    {
        #region Fields

        private double acceleration;

        private double deceleration;

        private double speed;

        #endregion

        #region Constructors

        public MovementParameters()
        {
        }

        public MovementParameters(MovementParameters param)
        {
            this.acceleration = param.Acceleration;
            this.deceleration = param.Deceleration;
            this.speed = param.Speed;
            this.Id = param.Id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the acceleration of the movement, in mm/s^2.
        /// </summary>
        public double Acceleration
        {
            get => this.acceleration;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.acceleration = value;
            }
        }

        /// <summary>
        /// Gets or sets the deceleration of the movement, in mm/s^2.
        /// </summary>
        public double Deceleration
        {
            get => this.deceleration;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.deceleration = value;
            }
        }

        /// <summary>
        /// Gets or sets the speed of the movement, in mm/s.
        /// </summary>
        public double Speed
        {
            get => this.speed;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.speed = value;
            }
        }

        #endregion
    }
}
