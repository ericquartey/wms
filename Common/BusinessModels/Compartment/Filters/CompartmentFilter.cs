using System;

namespace Ferretto.Common.BusinessModels
{
    public class CompartmentFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc = delegate (ICompartment compartment, ICompartment selected)
        {
            var compartmentDetails = compartment as CompartmentDetails;
            var selectedDetails = selected as CompartmentDetails;

            var color = "Red";
            if (selectedDetails != null && compartmentDetails != null)
            {
                if ((compartmentDetails.CompartmentTypeId != 0 || compartmentDetails == selectedDetails)
                    &&
                    compartmentDetails.CompartmentTypeId == selectedDetails.CompartmentTypeId)
                {
                    color = "#76FF03";
                }
                else
                {
                    color = "#90A4AE";
                }
            }
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => colorFunc;

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
