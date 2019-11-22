using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface IOperatorHub
    {
        #region Methods

        Task BayStatusChanged(IBayOperationalStatusChangedMessageData message);

        Task ErrorStatusChanged(int code);

        Task NewMissionOperationAvailable(BayNumber bayNumber, int missionId, int missionOperationId, int pendingMissionOperationsCount);

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
