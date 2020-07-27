using System;
using Ferretto.VW.Devices.WeightingScale;

namespace Ferretto.VW.App.Accessories.Interfaces.WeightingScale
{
    public sealed class WeightAcquiredEventArgs : EventArgs
    {
        #region Constructors

        public WeightAcquiredEventArgs(IWeightSample weightSample)
        {
            if (weightSample is null)
            {
                throw new ArgumentNullException(nameof(weightSample));
            }

            this.WeightSample = weightSample;
        }

        #endregion

        #region Properties

        public IWeightSample WeightSample { get; }

        #endregion
    }
}
