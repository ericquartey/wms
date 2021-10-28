namespace Ferretto.VW.MAS.DataModels
{
    public sealed class WeightMeasurement : DataModel
    {
        #region Constructors

        public WeightMeasurement()
        {
        }

        public WeightMeasurement(WeightMeasurement weightMeasurement)
        {
            this.MeasureConst0 = weightMeasurement.MeasureConst0;
            this.MeasureConst1 = weightMeasurement.MeasureConst1;
            this.MeasureConst2 = weightMeasurement.MeasureConst2;
            this.MeasureSpeed = weightMeasurement.MeasureSpeed;
            this.MeasureTime = weightMeasurement.MeasureTime;
            this.Id = weightMeasurement.Id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the k0 value in the formula:  weight = k2 * current^2 + k1 * current + k0.
        /// </summary>
        public double MeasureConst0 { get; set; }

        /// <summary>
        /// Gets or sets the k1 value in the formula:  weight = k2 * current^2 + k1 * current + k0.
        /// </summary>
        public double MeasureConst1 { get; set; }

        /// <summary>
        /// Gets or sets the k2 value in the formula:  weight = k2 * current^2 + k1 * current + k0.
        /// </summary>
        public double MeasureConst2 { get; set; }

        /// <summary>
        /// Gets or sets the speed for the upward movement during weight measurement, in millimeters/seconds.
        /// </summary>
        public double MeasureSpeed { get; set; }

        /// <summary>
        /// Gets or sets the time between the start of slow upward movement and the torque current request message, in tenth of seconds.
        /// </summary>
        public int MeasureTime { get; set; }

        #endregion
    }
}
