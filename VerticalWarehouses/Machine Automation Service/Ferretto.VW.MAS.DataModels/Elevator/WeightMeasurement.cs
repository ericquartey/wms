using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class WeightMeasurement : DataModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the kM value in the formula weight = kM * current + kS.
        /// </summary>
        public double MeasureMultiply { get; set; }

        /// <summary>
        /// Gets or sets the speed for the upward movement during weight measurement, in millimeters/meter.
        /// </summary>
        public double MeasureSpeed { get; set; }

        /// <summary>
        /// Gets or sets the kS value in the formula weight = kM * current + kS.
        /// </summary>
        public double MeasureSum { get; set; }

        /// <summary>
        /// Gets or sets the time between the start of slow upward movement and the torque current request message, in tenth of seconds.
        /// </summary>
        public int MeasureTime { get; set; }

        #endregion
    }
}
