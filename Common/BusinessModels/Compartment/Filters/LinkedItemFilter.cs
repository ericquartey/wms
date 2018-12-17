using System;

namespace Ferretto.Common.BusinessModels
{
    public class LinkedItemFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc = delegate (ICompartment compartment, ICompartment selected)
        {
            var compartmentDetails = compartment as CompartmentDetails;
            var selectedDetails = selected as CompartmentDetails;

            var color = "Blue";
            if (selected != null)
            {
                if (compartment == selected
                    &&
                    compartmentDetails.IsItemPairingFixed == selectedDetails.IsItemPairingFixed)
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
