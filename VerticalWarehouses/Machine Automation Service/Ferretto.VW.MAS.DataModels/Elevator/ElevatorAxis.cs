using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Fields

        private double lowerBound;

        private double offset;

        private decimal resolution;

        private int totalCycles = 1;

        private double upperBound = 1;

        #endregion

        #region Properties

        public double BrakeActivatePercent { get; set; }

        public double BrakeReleaseTime { get; set; }

        public double ChainOffset { get; set; }

        public MovementParameters EmptyLoadMovement { get; set; }

        public MovementParameters FullLoadMovement { get; set; }

        public Inverter Inverter { get; set; }

        public double LowerBound
        {
            get => this.lowerBound;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Lower bound cannot be negative.");
                }

                this.lowerBound = value;
            }
        }

        public double Offset
        {
            get => this.offset;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Offset cannot be negative.");
                }

                this.offset = value;
            }
        }

        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the target position for the horizontal movement in the profile calibration procedure.
        /// </summary>
        public double ProfileCalibrateLength { get; set; }

        /// <summary>
        /// Gets or sets the position that must match the raise of ProfileCalibrationBay signal in the profile calibration procedure.
        /// </summary>
        public int ProfileCalibratePosition { get; set; }

        /// <summary>
        /// Gets or sets the speed used in the profile calibration procedure.
        /// </summary>
        public double ProfileCalibrateSpeed { get; set; }

        public IEnumerable<MovementProfile> Profiles { get; set; }

        public decimal Resolution
        {
            get => this.resolution;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Resolution cannot be negative or zero.");
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
                    throw new ArgumentOutOfRangeException(nameof(value), "Total cycles cannot be negative or zero.");
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
                    throw new ArgumentOutOfRangeException(nameof(value), "Upper bound cannot be negative or zero.");
                }

                this.upperBound = value;
            }
        }

        public WeightMeasurement WeightMeasurement { get; set; }

        #endregion

        #region Methods

        public MovementParameters ScaleMovementsByWeight(LoadingUnit loadingUnit)
        {
            if (loadingUnit is null)
            {
                return this.EmptyLoadMovement;
            }

            var maxGrossWeight = loadingUnit.MaxNetWeight + loadingUnit.Tare;

            System.Diagnostics.Debug.Assert(
                maxGrossWeight > 0,
                "Max gross weight should always be positive (consistency ensured by LoadingUnit class).");

            var scalingFactor = loadingUnit.GrossWeight / maxGrossWeight;

            return new MovementParameters
            {
                Speed = this.EmptyLoadMovement.Speed - ((this.EmptyLoadMovement.Speed - this.FullLoadMovement.Speed) * scalingFactor),
                Acceleration = this.EmptyLoadMovement.Acceleration - ((this.EmptyLoadMovement.Acceleration - this.FullLoadMovement.Acceleration) * scalingFactor),
                Deceleration = this.EmptyLoadMovement.Deceleration - ((this.EmptyLoadMovement.Deceleration - this.FullLoadMovement.Deceleration) * scalingFactor),
            };
        }

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
