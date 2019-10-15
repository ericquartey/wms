using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IErrorsProvider
    {
        #region Methods

        Error GetCurrent();

        ErrorStatisticsSummary GetStatistics();

        Error RecordNew(MachineErrors code, BayNumber bayNumber = BayNumber.None);

        Error Resolve(int id);

        void ResolveAll();

        #endregion
    }
}
