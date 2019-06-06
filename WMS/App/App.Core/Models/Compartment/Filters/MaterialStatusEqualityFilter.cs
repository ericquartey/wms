using System;
using Ferretto.Common.Controls.WPF;

namespace Ferretto.WMS.App.Core.Models
{
    public class MaterialStatusEqualityFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => (compartment, selected) =>
        {
            if (compartment is IMaterialStatusCompartment typedCompartment
                && selected is IMaterialStatusCompartment selectedTypedCompartment)
            {
                return typedCompartment.MaterialStatusId == selectedTypedCompartment.MaterialStatusId ? "#76FF03" : "#90A4AE";
            }

            return "Orange";
        };

        public string Description => "MaterialStatus";

        public int Id => 1;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
