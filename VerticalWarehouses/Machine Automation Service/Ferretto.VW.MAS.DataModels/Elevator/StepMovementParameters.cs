using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class StepMovementParameters : MovementParameters
    {
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
                if (this.Speed >= axis.FullLoadMovement.Speed && this.Speed <= axis.EmptyLoadMovement.Speed)
                {
                    var deltaSpeed = (axis.EmptyLoadMovement.Speed - axis.FullLoadMovement.Speed) * scalingFactor;
                    this.Speed = Math.Max(this.Speed - deltaSpeed, axis.FullLoadMovement.Speed);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisSpeedRange, axis.Orientation, this.Speed));
                }
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
