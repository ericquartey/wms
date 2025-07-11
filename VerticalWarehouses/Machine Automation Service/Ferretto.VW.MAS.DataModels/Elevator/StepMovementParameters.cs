﻿using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class StepMovementParameters : MovementParameters
    {
        #region Constructors

        public StepMovementParameters()
        {
        }

        public StepMovementParameters(StepMovementParameters param)
        {
            this.AdjustAccelerationByWeight = param.AdjustAccelerationByWeight;
            this.AdjustSpeedByWeight = param.AdjustSpeedByWeight;
            this.Number = param.Number;
            this.Position = param.Position;
            this.Acceleration = param.Acceleration;
            this.Deceleration = param.Deceleration;
            this.Speed = param.Speed;
            this.Id = param.Id;
        }

        #endregion

        #region Properties

        public bool AdjustAccelerationByWeight { get; set; }

        public bool AdjustSpeedByWeight { get; set; }

        public int Number { get; set; }

        public double Position { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// In horizontal movements some steps can be scaled by weight, depending on Adjust___ByWeight parameters.
        /// </summary>
        /// <param name="adjustpositionbyweight">min value 0, max value 1.The higher is scalingFactor the lower goes speed/Acceleration </param>
        /// <param name="axis"></param>
        public void AdjustPositionByCenter(ElevatorAxis axis, MovementProfileType movementProfileType, int center)
        {
            if (axis is null)
            {
                throw new ArgumentNullException(nameof(axis));
            }
            if (axis.EmptyLoadMovement.Speed < axis.FullLoadMovement.Speed)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisSpeedConfiguration, axis.Orientation, this.Speed));
            }
            if (axis.EmptyLoadMovement.Acceleration < axis.FullLoadMovement.Acceleration)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisAccelerationConfiguration, axis.Orientation, this.Acceleration));
            }

            if (movementProfileType == MovementProfileType.LongPickup || movementProfileType == MovementProfileType.LongDeposit)
            {
                if (center != 0)
                {
                    this.Position += center;
                }
            }
            else if (movementProfileType == MovementProfileType.ShortDeposit || movementProfileType == MovementProfileType.ShortPickup)
            {
                if (center != 0)
                {
                    this.Position -= center;
                }
            }
        }

        /// <summary>
        /// In horizontal movements some steps can be scaled by weight, depending on Adjust___ByWeight parameters.
        /// </summary>
        /// <param name="scalingFactor">min value 0, max value 1.The higher is scalingFactor the lower goes speed/Acceleration </param>
        /// <param name="axis"></param>
        public void ScaleMovementsByWeight(double scalingFactor, ElevatorAxis axis)
        {
            if (axis is null)
            {
                throw new ArgumentNullException(nameof(axis));
            }
            if (axis.EmptyLoadMovement.Speed < axis.FullLoadMovement.Speed)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisSpeedConfiguration, axis.Orientation, this.Speed));
            }
            if (axis.EmptyLoadMovement.Acceleration < axis.FullLoadMovement.Acceleration)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisAccelerationConfiguration, axis.Orientation, this.Acceleration));
            }

            if (this.AdjustSpeedByWeight)
            {
                var highSpeed = Math.Min(this.Speed, axis.EmptyLoadMovement.Speed);
                var lowSpeed = Math.Min(this.Speed, axis.FullLoadMovement.Speed);
                var deltaSpeed = (highSpeed - lowSpeed) * scalingFactor;
                this.Speed = Math.Max(highSpeed - deltaSpeed, lowSpeed);
            }
            if (this.AdjustAccelerationByWeight)
            {
                if (this.Acceleration >= axis.FullLoadMovement.Acceleration && this.Acceleration <= axis.EmptyLoadMovement.Acceleration)
                {
                    var deltaAcceleration = (axis.EmptyLoadMovement.Acceleration - axis.FullLoadMovement.Acceleration) * scalingFactor;
                    this.Acceleration = Math.Max(this.Acceleration - deltaAcceleration, axis.FullLoadMovement.Acceleration);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisAccelerationRange, axis.Orientation, this.Acceleration));
                }
            }
        }

        #endregion
    }
}
