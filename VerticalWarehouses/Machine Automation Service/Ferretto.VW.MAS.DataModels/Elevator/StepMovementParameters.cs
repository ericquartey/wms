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
                if (this.Speed >= axis.FullLoadMovement.Speed && this.Speed <= axis.EmptyLoadMovement.Speed)
                {
                    var deltaSpeed = (axis.EmptyLoadMovement.Speed - axis.FullLoadMovement.Speed) * scalingFactor;
                    this.Speed = Math.Max(this.Speed - deltaSpeed, axis.FullLoadMovement.Speed);
                }

                if (this.Acceleration >= axis.FullLoadMovement.Acceleration && this.Acceleration <= axis.EmptyLoadMovement.Acceleration)
                {
                    var deltaAcceleration = (axis.EmptyLoadMovement.Acceleration - axis.FullLoadMovement.Acceleration) * scalingFactor;
                    this.Acceleration = Math.Max(this.Acceleration - deltaAcceleration, axis.FullLoadMovement.Acceleration);
                }
            }
        }

        #endregion
    }
}
