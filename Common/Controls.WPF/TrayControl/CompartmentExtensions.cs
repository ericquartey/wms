using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.Controls.WPF
{
    public static class CompartmentExtensions
    {
        #region Methods

        public static IEnumerable<CompartmentViewModel> AsCompartmentViewModel(this IEnumerable itemCollection)
        {
            if (itemCollection == null)
            {
                return new List<CompartmentViewModel>(0);
            }

            return itemCollection.Cast<CompartmentViewModel>();
        }

        #endregion
    }
}
