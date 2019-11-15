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
            if (this.AdjustByWeight)
            {
                this.Speed = axis.EmptyLoadMovement.Speed - ((axis.EmptyLoadMovement.Speed - axis.FullLoadMovement.Speed) * scalingFactor);
                this.Acceleration = axis.EmptyLoadMovement.Acceleration - ((axis.EmptyLoadMovement.Acceleration - axis.FullLoadMovement.Acceleration) * scalingFactor);
                this.Deceleration = axis.EmptyLoadMovement.Deceleration - ((axis.EmptyLoadMovement.Deceleration - axis.FullLoadMovement.Deceleration) * scalingFactor);
            }
        }

        #endregion
    }
}
