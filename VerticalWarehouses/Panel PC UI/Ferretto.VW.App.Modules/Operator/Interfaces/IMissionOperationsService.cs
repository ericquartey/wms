using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IMissionOperationsService
    {
        #region Properties

        Mission ActiveMachineMission { get; }

        MissionWithLoadingUnitDetails ActiveWmsMission { get; }

        MissionOperation ActiveWmsOperation { get; }

        #endregion

        #region Methods

        /// <exception cref="MasWebApiException"></exception>
        /// <exception cref="System.Net.Http.HttpRequestException"></exception>
        Task<bool> CompleteAsync(int operationId, double quantity, string barcode = null, double wastedQuantity = 0, string toteBarcode = null);

        Task<IEnumerable<ItemList>> GetAllMissionsMachineAsync();

        Task<IEnumerable<ProductInMachine>> GetProductsAsync(int? areaId, string itemCode, CancellationToken? cancellationToken = null);

        Task<bool> IsLastRowForListAsync(string itemListCode);

        Task<bool> IsLastWmsMissionForCurrentLoadingUnitAsync(int missionId);

        Task<string> IsMultiMachineAsync(int missionId);

        bool IsRecallLoadingUnitId();

        Task<bool> MustCheckToteBarcode();

        /// <exception cref="MasWebApiException"></exception>
        /// <exception cref="System.Net.Http.HttpRequestException"></exception>
        Task<bool> PartiallyCompleteAsync(int operationId, double quantity, double wastedQuantity, string printerName, bool? emptyCompartment, bool? fullCompartment);

        Task RecallLoadingUnitAsync(int id);

        int RecallLoadingUnitId();

        Task<IEnumerable<Mission>> RefreshAsync(bool force = false);

        Task StartAsync();

        Task<bool> SuspendAsync(int operationId);

        #endregion
    }
}
