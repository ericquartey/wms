using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IErrorsProvider
    {
        #region Methods

        MachineError GetCurrent();

        List<MachineError> GetErrors();

        ErrorStatisticsSummary GetStatistics();

        bool IsErrorSmall();

        MachineError RecordNew(int inverterIndex, ushort detailCode, BayNumber bayNumber = BayNumber.None);

        MachineError RecordNew(MachineErrorCode code, BayNumber bayNumber = BayNumber.None, string additionalText = null);

        MachineError Resolve(int id);

        void ResolveAll();

        #endregion
    }
}
