using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface IOperatorHub
    {
        #region Methods

        Task BayStatusChanged(BayOperationalStatusChangedMessageData message);

        Task NewMissionOperationAvailable(MissionOperationInfo missionOperation);

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
