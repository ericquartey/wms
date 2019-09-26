namespace Ferretto.VW.MAS.DataModels
{
    public class MovementParameters : DataModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the acceleration of the movement, in mm/s^2.
        /// </summary>
        public double Acceleration { get; set; }

        /// <summary>
        /// Gets or sets the deceleration of the movement, in mm/s^2.
        /// </summary>
        public double Deceleration { get; set; }

        /// <summary>
        /// Gets or sets the speed of the movement, in mm/s.
        /// </summary>
        public double Speed { get; set; }

        #endregion
    }
}
