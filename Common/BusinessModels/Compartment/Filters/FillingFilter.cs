using System;

namespace Ferretto.Common.BusinessModels
{
    public class FillingFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            var color = "#00000000";
            var compartmentDetails = compartment as CompartmentDetails;

            if (compartmentDetails?.MaxCapacity.HasValue == false)
            {
                return color;
            }

            var fillRatio = (double)compartmentDetails.Stock / compartmentDetails.MaxCapacity.Value;
            if (fillRatio <= 0)
            {
                color = "#FF63BE7B";
            }
            else if (fillRatio < 0.1)
            {
                color = "#FF80C77D";
            }
            else if (fillRatio < 0.2)
            {
                color = "#FF9CCF7F";
            }
            else if (fillRatio < 0.3)
            {
                color = "#FFB9D780";
            }
            else if (fillRatio < 0.4)
            {
                color = "#FFD5DF82";
            }
            else if (fillRatio < 0.5)
            {
                color = "#FFF1E784";
            }
            else if (fillRatio < 0.6)
            {
                color = "#FFFEDF81";
            }
            else if (fillRatio < 0.7)
            {
                color = "#FFFDC77D";
            }
            else if (fillRatio < 0.8)
            {
                color = "#FFFBAF78";
            }
            else if (fillRatio < 0.9)
            {
                color = "#FFFA9874";
            }
            else if (fillRatio < 1)
            {
                color = "#FFF9806F";
            }
            else
            {
                color = "#FFF8696B";
            }

            return color;
        };

        public string Description => "Fill Ratio";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
