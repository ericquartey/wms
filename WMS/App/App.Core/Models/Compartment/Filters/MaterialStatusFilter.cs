using System;

namespace Ferretto.WMS.App.Core.Models
{
    public class MaterialStatusFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            var compartmentDetails = compartment as CompartmentDetails;
            var selectedDetails = selected as CompartmentDetails;
            if (selectedDetails != null && compartmentDetails != null)
            {
                if (compartmentDetails.MaterialStatusId == selectedDetails.MaterialStatusId)
                {
                    return "#76FF03";
                }
                else
                {
                    return "#90A4AE";
                }
            }

            return "Orange";
        };

        public string Description => "MaterialStatus";

        public int Id => 1;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
