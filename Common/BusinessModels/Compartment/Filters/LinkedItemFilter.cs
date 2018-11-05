using System;

namespace Ferretto.Common.BusinessModels
{
    public class LinkedItemFilter : IFilter
    {
        #region Fields

        private static readonly Func<CompartmentDetails, CompartmentDetails, string> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            var color = "Blue";
            if (selected != null)
            {
                if ((compartment.ItemPairing != 0 || compartment == selected) && compartment.ItemPairing == selected.ItemPairing)
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

        public Func<CompartmentDetails, CompartmentDetails, string> ColorFunc => colorFunc;
        public string Description => "Compartment";
        public int Id => 2;
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }
}
