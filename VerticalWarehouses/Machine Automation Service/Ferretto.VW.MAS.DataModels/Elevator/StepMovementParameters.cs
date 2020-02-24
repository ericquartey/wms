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

        /// <summary>
        /// In horizontal movements some steps can be scaled by weight, depending on AdjustByWeight parameter.
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
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisSpeedConfiguration, axis.Orientation));
            }
            if (axis.EmptyLoadMovement.Acceleration < axis.FullLoadMovement.Acceleration)
            {
                throw new InvalidOperationException(string.Format(Resources.ErrorReasons.InvalidAxisAccelerationConfiguration, axis.Orientation));
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
