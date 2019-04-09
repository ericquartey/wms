using System;
using Ferretto.WMS.App.Controls;

namespace Ferretto.WMS.App.Core.Models
{
    public class LinkedItemFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            if (compartment is IPairedCompartment pairedCompartment
                && selected is IPairedCompartment selectedPairedCompartment)
            {
                if (pairedCompartment.IsItemPairingFixed == selectedPairedCompartment.IsItemPairingFixed)
                {
                    return "#76FF03";
                }
                else
                {
                    return "#90A4AE";
                }
            }

            return "Blue";
        };

        public string Description => "Compartment";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
