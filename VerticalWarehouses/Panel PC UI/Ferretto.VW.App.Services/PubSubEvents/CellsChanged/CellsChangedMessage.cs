using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class CellsChangedMessage
    {
        #region Constructors

        public CellsChangedMessage(IEnumerable<Cell> cells)
        {
            this.Cells = cells;
        }

        #endregion

        #region Properties

        public IEnumerable<Cell> Cells { get; }

        #endregion
    }
}
