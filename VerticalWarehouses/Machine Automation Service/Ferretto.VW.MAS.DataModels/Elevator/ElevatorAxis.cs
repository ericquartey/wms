using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Fields

        private double offset;

        private decimal resolution;

        private int totalCycles = 1;

        private double upperBound;

        #endregion

        #region Properties

        public double ChainOffset { get; set; }

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
                    throw new System.ArgumentOutOfRangeException(nameof(value), "Offset cannot be negative.");
                }

                this.offset = value;
            }
        }

        public Orientation Orientation { get; set; }

        public IEnumerable<MovementProfile> Profiles { get; set; }

        public decimal Resolution
        {
            get => this.resolution;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value), "Resolution cannot be negative or zero.");
                }

                this.resolution = value;
            }
        }

        public int TotalCycles
        {
            get => this.totalCycles;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value), "Total cycles cannot be negative or zero.");
                }

                this.totalCycles = value;
            }
        }

        public double UpperBound
        {
            get => this.upperBound;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "UpperBound cannot be negative or zero.");
                }

                this.upperBound = value;
            }
        }

        #endregion

        #region Methods

        public MovementParameters ScaleMovementsByWeight(double grossWeight, double maximumLoadOnBoard)
        {
            if (grossWeight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(grossWeight));
            }

            if (maximumLoadOnBoard <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(grossWeight));
            }

            if (grossWeight > maximumLoadOnBoard)
            {
                throw new ArgumentOutOfRangeException(nameof(grossWeight));
            }

            var maximumLoadMovement = this.MaximumLoadMovement;
            var emptyLoadMovement = this.EmptyLoadMovement;

            var scalingFactor = grossWeight / maximumLoadOnBoard;

            return new MovementParameters
            {
                Speed = emptyLoadMovement.Speed + ((emptyLoadMovement.Speed - maximumLoadMovement.Speed) * scalingFactor),
                Acceleration = emptyLoadMovement.Acceleration + ((emptyLoadMovement.Acceleration - maximumLoadMovement.Acceleration) * scalingFactor),
                Deceleration = emptyLoadMovement.Deceleration + ((emptyLoadMovement.Deceleration - maximumLoadMovement.Deceleration) * scalingFactor),
            };
        }

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
