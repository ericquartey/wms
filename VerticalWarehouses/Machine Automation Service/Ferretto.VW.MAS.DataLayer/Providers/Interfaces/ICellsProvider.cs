using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ICellsProvider
    {
        #region Methods

        IEnumerable<Cell> GetAll();

        CellStatisticsSummary GetStatistics();

        void LoadFrom(string fileNamePath);

        Cell UpdateHeight(int cellId, decimal height);

        #endregion
    }
}
