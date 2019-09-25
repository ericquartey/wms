using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Fields

        private decimal offset;

        private decimal resolution;

        #endregion

        #region Properties

        public MovementParameters EmptyLoadMovement { get; set; }

        public Inverter Inverter { get; set; }

        public decimal LowerBound { get; set; }

        public MovementParameters MaximumLoadMovement { get; set; }

        public decimal Offset
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
        public decimal Position { get; set; }

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

        public decimal UpperBound { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
