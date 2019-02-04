using System.Threading.Tasks;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal interface IAutomationService
    {
        #region Methods

        Task InitializeAsync();

        Task NotifyUserLoginAsync(int bayId);

        #endregion
    }
}
