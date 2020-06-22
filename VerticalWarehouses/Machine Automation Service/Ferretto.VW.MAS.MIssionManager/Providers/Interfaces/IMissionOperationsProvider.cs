﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id);

        Task CompleteAsync(int wmsId, double quantity, string printerName);

        Task<MissionOperation> GetByIdAsync(int wmsId);

        int GetCountByBay(BayNumber bayNumber);

        Task<IEnumerable<OperationReason>> GetReasonsAsync(MissionOperationType type);

        int GetUnitId(int missionId, BayNumber bayNumber);

        Task PartiallyCompleteAsync(int wmsId, double quantity, string printerName);

        #endregion
    }
}
