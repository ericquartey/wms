﻿using System;
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

        /// <summary>
        /// it is the kM value in the formula weight = kM * current + kS
        /// </summary>
        public double WeightMeasureMultiply { get; set; }

        /// <summary>
        /// the speed for the upward movement during weight measurement, in millimeters/meter.
        /// </summary>
        public double WeightMeasureSpeed { get; set; }

        /// <summary>
        /// it is the kS value in the formula weight = kM * current + kS
        /// </summary>
        public double WeightMeasureSum { get; set; }

        /// <summary>
        /// the time between the start of slow upward movement and the torque current request message, in tenth of seconds.
        /// </summary>
        public int WeightMeasureTime { get; set; }

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

            var scalingFactor = grossWeight / maximumLoadOnBoard;

            return new MovementParameters
            {
                Speed = this.EmptyLoadMovement.Speed + ((this.EmptyLoadMovement.Speed - this.FullLoadMovement.Speed) * scalingFactor),
                Acceleration = this.EmptyLoadMovement.Acceleration + ((this.EmptyLoadMovement.Acceleration - this.FullLoadMovement.Acceleration) * scalingFactor),
                Deceleration = this.EmptyLoadMovement.Deceleration + ((this.EmptyLoadMovement.Deceleration - this.FullLoadMovement.Deceleration) * scalingFactor),
            };
        }

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
