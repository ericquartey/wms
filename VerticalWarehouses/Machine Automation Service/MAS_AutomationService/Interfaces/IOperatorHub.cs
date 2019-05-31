using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IOperatorHub
    {
        #region Methods

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick();

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
