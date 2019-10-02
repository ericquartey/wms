using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Fields

        private double offset;

        private decimal resolution;

        #endregion

        #region Properties

        public MovementParameters EmptyLoadMovement { get; set; }

        public Inverter Inverter { get; set; }

        public double LowerBound { get; set; }

        public MovementParameters MaximumLoadMovement { get; set; }

        public double Offset
        {
            get => this.offset;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException("Offset cannot be negative.", nameof(value));
                }

                this.offset = value;
            }
        }

        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the last known position of the elevator's axis.
        /// </summary>
        public double Position { get; set; }

        public IEnumerable<MovementProfile> Profiles { get; set; }

        public decimal Resolution
        {
            get => this.resolution;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException("Resolution cannot be negative or zero.", nameof(value));
                }

                this.resolution = value;
            }
        }

        public double UpperBound { get; set; }

        public int TotalCycles { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
