using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Fields

        private double lowerBound;

        private double offset;

        private double resolution;

        private double upperBound = 1;

        #endregion

        #region Properties

        public ElevatorAxisManualParameters AssistedMovements { get; set; }

        public double BrakeActivatePercent { get; set; }

        public double BrakeReleaseTime { get; set; }

        public double ChainOffset { get; set; }

        public MovementParameters EmptyLoadMovement { get; set; }

        public MovementParameters FullLoadMovement { get; set; }

        public double HomingCreepSpeed { get; set; }

        public double HorizontalCalibrateSpeed { get; set; }

        public double HomingFastSpeed { get; set; }

        public Inverter Inverter { get; set; }

        public int LastCalibrationCycles { get; set; }

        public double LastIdealPosition { get; set; }

        public double LowerBound
        {
            get => this.lowerBound;
            set
            {
                this.lowerBound = value;
            }
        }

        public ElevatorAxisManualParameters ManualMovements { get; set; }

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

        public double Resolution
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
        /// this parameter is added to the belt elongation
        /// </summary>
        public double? VerticalDepositOffset { get; set; }

        /// <summary>
        /// this parameter is added when positioning for pickup mission
        /// </summary>
        public double? VerticalPickupOffset { get; set; }

        public WeightMeasurement WeightMeasurement { get; set; }

        #endregion

        #region Methods

        public MovementParameters ScaleMovementsByWeight(LoadingUnit loadingUnit, bool isLoadingUnitOnBoard)
        {
            if (loadingUnit is null)
            {
                return isLoadingUnitOnBoard ? this.FullLoadMovement : this.EmptyLoadMovement;
            }
            if (this.EmptyLoadMovement.Speed < this.FullLoadMovement.Speed)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorDescriptions.AxisMovementSpeedConfigurationInvalid, this.Orientation));
            }
            if (this.EmptyLoadMovement.Acceleration < this.FullLoadMovement.Acceleration)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorDescriptions.AxisMovementAccelerationConfigurationInvalid, this.Orientation));
            }
            if (this.EmptyLoadMovement.Deceleration < this.FullLoadMovement.Deceleration)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorDescriptions.AxisMovementDecelerationConfigurationInvalid, this.Orientation));
            }

            var maxGrossWeight = loadingUnit.MaxNetWeight + loadingUnit.Tare;

            System.Diagnostics.Debug.Assert(
                maxGrossWeight > 0,
                "Max gross weight should always be positive (consistency ensured by LoadingUnit class).");

            // if weight is unknown we move as full weight
            // min value 0, max value 1.The higher is scalingFactor the lower goes speed/Acceleration/Deceleration
            var scalingFactor = (loadingUnit.GrossWeight == 0 || loadingUnit.GrossWeight > maxGrossWeight) ? 1 : (loadingUnit.GrossWeight / maxGrossWeight);

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
