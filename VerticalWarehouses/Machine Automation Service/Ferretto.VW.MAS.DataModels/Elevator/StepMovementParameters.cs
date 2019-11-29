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
                var deltaSpeed = (Math.Min(axis.EmptyLoadMovement.Speed, this.Speed) - Math.Min(axis.FullLoadMovement.Speed, this.Speed)) * scalingFactor;
                this.Speed = Math.Max(this.Speed - deltaSpeed, axis.FullLoadMovement.Speed);

                var deltaAcceleration = (Math.Min(axis.EmptyLoadMovement.Acceleration, this.Acceleration) - Math.Min(axis.FullLoadMovement.Acceleration, this.Acceleration)) * scalingFactor;
                this.Acceleration = Math.Max(this.Acceleration - deltaAcceleration, axis.FullLoadMovement.Acceleration);
            }
        }

        #endregion
    }
}
