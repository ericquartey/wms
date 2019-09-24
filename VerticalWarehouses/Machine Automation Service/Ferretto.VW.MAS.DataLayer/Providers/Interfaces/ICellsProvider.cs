using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ICellsProvider
    {
        #region Methods

        IEnumerable<Cell> GetAll();

        Models.CellStatisticsSummary GetStatistics();

        Cell UpdateHeight(int cellId, decimal height);

        #endregion
    }
}
