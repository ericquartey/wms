using System;

namespace Ferretto.Common.BusinessModels
{
    public class EditFilter : IFilter
    {
        #region Fields

        private static readonly Func<CompartmentDetails, CompartmentDetails, string> colorFunc =
            delegate (CompartmentDetails compartment, CompartmentDetails selected)
            {
                var color = "#57A639";

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
