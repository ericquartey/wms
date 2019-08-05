using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ICellsProvider
    {
        #region Methods

        CellStatisticsSummary GetStatistics();

        #endregion
    }
}
