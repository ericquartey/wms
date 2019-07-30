using Ferretto.VW.MAS.DataModels.Errors;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IErrorsProvider
    {
        #region Methods

        Error GetCurrent();

        ErrorStatisticsSummary GetStatistics();

        #endregion
    }
}
