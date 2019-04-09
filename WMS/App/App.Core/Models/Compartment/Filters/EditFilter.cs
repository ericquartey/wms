using System;
using Ferretto.WMS.App.Controls;

namespace Ferretto.WMS.App.Core.Models
{
    public class EditFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            return "#57A639";
        };

        public string Description => "Compartment";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
