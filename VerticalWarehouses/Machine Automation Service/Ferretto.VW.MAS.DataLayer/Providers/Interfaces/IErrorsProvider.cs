using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IErrorsProvider
    {
        #region Methods

        MachineError GetById(int id);

        MachineError GetCurrent();

        List<MachineError> GetErrors();

        MachineError GetLast();

        ErrorStatisticsSummary GetStatistics();

        //bool IsErrorSmall();
        bool NeedsHoming();

        int PurgeErrors();

        MachineError RecordNew(int inverterIndex, ushort detailCode, BayNumber bayNumber = BayNumber.None, string detailText = null);

        MachineError RecordNew(MachineErrorCode code, BayNumber bayNumber = BayNumber.None, string additionalText = null);

        MachineError Resolve(int id, bool force = false);

        void ResolveAll(bool force = false);

        #endregion
    }
}
