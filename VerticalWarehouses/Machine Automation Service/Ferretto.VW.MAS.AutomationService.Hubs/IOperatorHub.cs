using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface IOperatorHub
    {
        #region Methods

        Task AssignedMissionChanged(BayNumber bayNumber, int? missionId);

        Task AssignedMissionOperationChanged(BayNumber bayNumber);

        Task BayStatusChanged(BayNumber bayNumber, BayStatus bayStatus);

        Task ErrorStatusChanged(int code);

        Task ProductsChanged();

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick(IBaseNotificationMessageUI message);

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting(IBaseNotificationMessageUI message);

        #endregion
    }
}
