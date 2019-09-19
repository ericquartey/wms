namespace Ferretto.VW.MAS.DataModels
{
    public class MovementParameters : DataModel
    {
        #region Properties

        /// <summary>
        /// The acceleration of the movement, in mm/s^2.
        /// </summary>
        public decimal Acceleration { get; set; }

        /// <summary>
        /// The deceleration of the movement, in mm/s^2.
        /// </summary>
        public decimal Deceleration { get; set; }

        /// <summary>
        /// The speed of the movement, in mm/s.
        /// </summary>
        public decimal Speed { get; set; }

        #endregion
    }
}
