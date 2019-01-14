using System;

namespace Ferretto.Common.BusinessModels
{
    public class EditReadOnlyFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            var color = "#e6e6e6";
            return color;
        };

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
