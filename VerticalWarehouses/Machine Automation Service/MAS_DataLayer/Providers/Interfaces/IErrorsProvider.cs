using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IErrorsProvider
    {
        #region Methods

        Error GetCurrent();

        ErrorStatisticsSummary GetStatistics();

        Error RecordNew(MachineErrors code);

        Error Resolve(int id);

        #endregion
    }
}
