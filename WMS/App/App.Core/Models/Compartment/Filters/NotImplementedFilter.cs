using System;
using Ferretto.Common.Controls.WPF;

namespace Ferretto.WMS.App.Core.Models
{
    public class NotImplementedFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) => "Gray";

        public string Description => "NotImplemented";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
