using System;

namespace Ferretto.Common.BusinessModels
{
    public class EditReadOnlyFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            return "#e6e6e6";
        };

        public string Description => "EditReadOnly";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
