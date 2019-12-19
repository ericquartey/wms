using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class LoadUnitsChangedMessage
    {
        #region Constructors

        public LoadUnitsChangedMessage(IEnumerable<LoadingUnit> loadUnits)
        {
            this.LoadUnits = loadUnits;
        }

        #endregion

        #region Properties

        public IEnumerable<LoadingUnit> LoadUnits { get; }

        #endregion
    }
}
