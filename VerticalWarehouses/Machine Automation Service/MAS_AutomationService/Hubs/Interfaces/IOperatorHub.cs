using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Hubs.Interfaces
{
    public interface IOperatorHub
    {
        #region Methods

        Task OnConnectionEstablished(IBaseNotificationMessageUI message);

        Task ProvideMissionsToBay(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
