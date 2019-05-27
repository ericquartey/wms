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
            return itemCollection == null ?
                new List<CompartmentViewModel>(0) :
                itemCollection.Cast<CompartmentViewModel>();
        }

        #endregion
    }
}
