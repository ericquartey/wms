using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public class CarouselManualParameters : DataModel
    {
        #region Fields

        private double feedRate = 0.15;

        #endregion

        #region Properties

        public double FeedRate
        {
            get => this.feedRate;
            set
            {
                if (value <= 0 || value > 1)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.feedRate = value;
            }
        }

        #endregion
    }
}
