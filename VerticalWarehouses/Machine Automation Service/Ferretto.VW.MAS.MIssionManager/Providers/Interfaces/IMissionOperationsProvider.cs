using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Migrations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionManager
{
    public interface IMissionOperationsProvider
    {
        #region Methods

        Task AbortAsync(int id, string userName = null);

        Task CompleteAsync(int wmsId, double quantity, string printerName, string barcode = null, double wastedQuantity = 0, string toteBarcode = null, string userName = null, int? nrLabels = null);

        Task<MissionOperation> GetByIdAsync(int wmsId);

        //Task<MissionOperation> GetByAggregateAsync(int wmsId);

        int GetCountByBay(BayNumber bayNumber);

        Task<IEnumerable<OperationReason>> GetOrdersAsync();

        Task<IEnumerable<MissionOperation>> GetPutListsAsync(int machineId);

        Task<IEnumerable<OperationReason>> GetReasonsAsync(MissionOperationType type);

        int GetUnitId(int missionId, BayNumber bayNumber);

        Task PartiallyCompleteAsync(int wmsId, double quantity, double wastedQuantity, string printerName, bool emptyCompartment = false, bool fullCompartment = false, string userName = null, int? nrLabels = null);

        Task<MissionOperation> SuspendAsync(int id, string userName = null);

        #endregion
    }
}
