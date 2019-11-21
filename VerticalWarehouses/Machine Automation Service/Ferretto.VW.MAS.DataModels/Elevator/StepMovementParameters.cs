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
                if (this.Speed - deltaSpeed > axis.FullLoadMovement.Speed)
                {
                    this.Speed -= deltaSpeed;
                }

                var deltaAcceleration = (axis.EmptyLoadMovement.Acceleration - axis.FullLoadMovement.Acceleration) * scalingFactor;
                if (this.Acceleration - deltaAcceleration > axis.FullLoadMovement.Acceleration)
                {
                    this.Acceleration -= deltaAcceleration;
                }
            }
        }

        #endregion
    }
}
