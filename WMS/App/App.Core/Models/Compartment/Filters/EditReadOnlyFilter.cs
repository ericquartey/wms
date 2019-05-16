using System;
using Ferretto.Common.Controls.WPF;

namespace Ferretto.WMS.App.Core.Models
{
    public class EditReadOnlyFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            return "#e6e6e6";
        };

        public string Description => "EditReadOnly";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
