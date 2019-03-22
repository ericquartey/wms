using System;

namespace Ferretto.WMS.App.Core.Models
{
    public class CompartmentFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            var compartmentDetails = compartment as CompartmentDetails;
            var selectedDetails = selected as CompartmentDetails;

            if (selectedDetails != null && compartmentDetails != null)
            {
                if ((compartmentDetails.CompartmentTypeId != 0 || compartmentDetails == selectedDetails)
                    &&
                    compartmentDetails.CompartmentTypeId == selectedDetails.CompartmentTypeId)
                {
                    return "#76FF03";
                }
                else
                {
                    return "#90A4AE";
                }
            }

            return "Red";
        };

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
