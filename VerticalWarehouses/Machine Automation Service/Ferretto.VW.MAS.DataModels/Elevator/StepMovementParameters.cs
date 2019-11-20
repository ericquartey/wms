using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class StepMovementParameters : MovementParameters
    {
        #region Properties

        public bool AdjustByWeight { get; set; }

        public int Number { get; set; }

        public double Position { get; set; }

        #endregion

        #region Methods

        public void ScaleMovementsByWeight(double scalingFactor, ElevatorAxis axis)
        {
            if (axis is null)
            {
                throw new ArgumentNullException(nameof(axis));
            }
            if (this.AdjustByWeight)
            {
                var deltaSpeed = (axis.EmptyLoadMovement.Speed - axis.FullLoadMovement.Speed) * scalingFactor;
                if (deltaSpeed < this.Speed)
                {
                    this.Speed -= deltaSpeed;
                }
                else
                {
                    this.Speed = axis.FullLoadMovement.Speed;
                }

                var deltaAcceleration = (axis.EmptyLoadMovement.Acceleration - axis.FullLoadMovement.Acceleration) * scalingFactor;
                if (deltaAcceleration < this.Acceleration)
                {
                    this.Acceleration -= deltaAcceleration;
                }
                else
                {
                    this.Acceleration = axis.FullLoadMovement.Acceleration;
                }

                //var deltaDeceleration = (axis.EmptyLoadMovement.Deceleration - axis.FullLoadMovement.Deceleration) * scalingFactor;
                //if (deltaDeceleration > this.Deceleration)
                //{
                //    this.Deceleration -= deltaDeceleration;
                //}
            }
        }

        #endregion
    }
}
