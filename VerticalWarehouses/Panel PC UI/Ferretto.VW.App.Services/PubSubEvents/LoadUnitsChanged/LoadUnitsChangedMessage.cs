using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class LoadUnitsChangedMessage
    {
        #region Constructors

        public LoadUnitsChangedMessage(IEnumerable<LoadingUnit> loadUnits)
        {
            this.Loadunits = loadUnits;
        }

        #endregion

        #region Properties

        public IEnumerable<LoadingUnit> Loadunits { get; }

        #endregion
    }
}
