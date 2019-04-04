using System;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.App.Core.Models
{
    public class CompartmentFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            var compartmentDetails = compartment as ITypedCompartment;
            var selectedDetails = selected as ITypedCompartment;

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

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
