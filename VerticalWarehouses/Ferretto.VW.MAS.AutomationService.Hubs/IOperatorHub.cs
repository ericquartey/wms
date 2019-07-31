using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Hubs.Interfaces
{
    public interface IOperatorHub
    {
        #region Methods

        Task BayStatusChanged(IBayOperationalStatusChangedMessageData message);

        Task ErrorStatusChanged(int code);

        Task NewMissionOperationAvailable(INewMissionOperationAvailable message);

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
