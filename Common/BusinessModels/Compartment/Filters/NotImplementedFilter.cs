using System;

namespace Ferretto.Common.BusinessModels
{
    public class NotImplementedFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) => "Gray";

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
