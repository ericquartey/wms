using System;

namespace Ferretto.Common.BusinessModels
{
    public class EditFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            return "#57A639";
        };

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
