using System;

namespace Ferretto.WMS.App.Core.Models
{
    public class MaterialStatusEqualityFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            if (compartment is IMaterialStatusCompartment typedCompartment
                && selected is IMaterialStatusCompartment selectedTypedCompartment)
            {
                if (typedCompartment.MaterialStatusId == selectedTypedCompartment.MaterialStatusId)
                {
                    return "#76FF03";
                }
                else
                {
                    return "#90A4AE";
                }
            }

            return "Orange";
        };

        public string Description => "MaterialStatus";

        public int Id => 1;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
