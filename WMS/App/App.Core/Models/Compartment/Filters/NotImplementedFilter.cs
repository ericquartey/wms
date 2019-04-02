using System;

namespace Ferretto.WMS.App.Core.Models
{
    public class NotImplementedFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) => "Gray";

        public string Description => "NotImplemented";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
