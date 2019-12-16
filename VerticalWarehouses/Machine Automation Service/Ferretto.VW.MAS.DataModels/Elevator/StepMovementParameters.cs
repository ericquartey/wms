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
                var min = Math.Min(axis.FullLoadMovement.Speed, this.Speed);
                var max = Math.Max(axis.EmptyLoadMovement.Speed, this.Speed);

                var deltaSpeed = (max - min) * scalingFactor;
                this.Speed = Math.Max(this.Speed - deltaSpeed, axis.FullLoadMovement.Speed);

                min = Math.Min(axis.FullLoadMovement.Acceleration, this.Acceleration);
                max = Math.Max(axis.EmptyLoadMovement.Acceleration, this.Acceleration);

                var deltaAcceleration = (max - min) * scalingFactor;
                this.Acceleration = Math.Max(this.Acceleration - deltaAcceleration, axis.FullLoadMovement.Acceleration);
            }
        }

        #endregion
    }
}
