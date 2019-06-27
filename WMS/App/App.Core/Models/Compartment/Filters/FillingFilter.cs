using System;
using Ferretto.Common.Controls.WPF;

namespace Ferretto.WMS.App.Core.Models
{
    public class FillingFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            var compartmentDetails = compartment as ICapacityCompartment;
            if (compartmentDetails == null || !compartmentDetails.MaxCapacity.HasValue)
            {
                return "#FF90A4AE";
            }

            var fillRatio = compartmentDetails.Stock / compartmentDetails.MaxCapacity.Value;

            switch (fillRatio)
            {
                case var ratio when ratio <= 0: return "#FF63BE7B";
                case var ratio when ratio < 0.1: return "#FF80C77D";
                case var ratio when ratio < 0.2: return "#FF9CCF7F";
                case var ratio when ratio < 0.3: return "#FFB9D780";
                case var ratio when ratio < 0.4: return "#FFD5DF82";
                case var ratio when ratio < 0.5: return "#FFF1E784";
                case var ratio when ratio < 0.6: return "#FFFEDF81";
                case var ratio when ratio < 0.7: return "#FFFDC77D";
                case var ratio when ratio < 0.8: return "#FFFBAF78";
                case var ratio when ratio < 0.9: return "#FFFA9874";
                case var ratio when ratio < 1.0: return "#FFF9806F";
                default: return "#FFF8696B";
            }
        };

        public string Description => "Fill Ratio";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
