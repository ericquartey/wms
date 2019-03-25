using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.Controls.Controls
{
    public static class CompartmentExtensions
    {
        #region Methods

        public static IEnumerable<WmsCompartmentViewModel> AsCompartmentViewModel(this IEnumerable itemCollection)
        {
            if (itemCollection == null)
            {
                return new List<WmsCompartmentViewModel>(0);
            }

            return itemCollection.Cast<WmsCompartmentViewModel>();
        }

        #endregion
    }
}
