using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IErrorsProvider
    {
        #region Methods

        Error GetCurrent();

        ErrorStatisticsSummary GetStatistics();

        Error RecordNew(MachineErrors code, BayNumber bayIndex);

        Error Resolve(int id);

        #endregion
    }
}
