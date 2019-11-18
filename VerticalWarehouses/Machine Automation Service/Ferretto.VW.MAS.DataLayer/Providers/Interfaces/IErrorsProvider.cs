using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IErrorsProvider
    {
        #region Methods

        MachineError GetCurrent();

        ErrorStatisticsSummary GetStatistics();

        MachineError RecordNew(MachineErrorCode code, BayNumber bayNumber = BayNumber.None);

        MachineError Resolve(int id);

        void ResolveAll();

        #endregion
    }
}
