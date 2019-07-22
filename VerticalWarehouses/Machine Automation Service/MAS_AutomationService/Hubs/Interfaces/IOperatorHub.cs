using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Interfaces
{
    public interface IOperatorHub
    {
        #region Methods

        Task BayStatusChanged(BayConnectedMessageData message);

        Task ProvideMissionsToBay(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
