using System;

namespace Ferretto.Common.BusinessModels
{
    public class FillingFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            var compartmentDetails = compartment as CompartmentDetails;

            if (compartmentDetails?.MaxCapacity.HasValue == false)
            {
                return "#00000000";
            }

            var fillRatio = (double)compartmentDetails.Stock / compartmentDetails.MaxCapacity.Value;
            if (fillRatio <= 0)
            {
                return "#FF63BE7B";
            }
            else if (fillRatio < 0.1)
            {
                return "#FF80C77D";
            }
            else if (fillRatio < 0.2)
            {
                return "#FF9CCF7F";
            }
            else if (fillRatio < 0.3)
            {
                return "#FFB9D780";
            }
            else if (fillRatio < 0.4)
            {
                return "#FFD5DF82";
            }
            else if (fillRatio < 0.5)
            {
                return "#FFF1E784";
            }
            else if (fillRatio < 0.6)
            {
                return "#FFFEDF81";
            }
            else if (fillRatio < 0.7)
            {
                return "#FFFDC77D";
            }
            else if (fillRatio < 0.8)
            {
                return "#FFFBAF78";
            }
            else if (fillRatio < 0.9)
            {
                return "#FFFA9874";
            }
            else if (fillRatio < 1)
            {
                return "#FFF9806F";
            }
            else
            {
                return "#FFF8696B";
            }
        };

        public string Description => "Fill Ratio";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
