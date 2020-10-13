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
        Task<bool> CompleteAsync(int operationId, double quantity, string barcode = null);

        /// <exception cref="MasWebApiException"></exception>
        /// <exception cref="System.Net.Http.HttpRequestException"></exception>
        Task<bool> PartiallyCompleteAsync(int operationId, double quantity);

        Task RecallLoadingUnitAsync(int id);

        int RecallLoadingUnitId();

        bool IsRecallLoadingUnitId();

        Task RefreshAsync();

        Task StartAsync();

        #endregion
    }
}
