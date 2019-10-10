using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ICellsProvider
    {
        #region Methods

        IEnumerable<Cell> GetAll();

        CellStatisticsSummary GetStatistics();

        Cell UpdateHeight(int cellId, double height);

        #endregion

        double
    }
}
