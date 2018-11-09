using System.Threading.Tasks;

namespace Ferretto.WMS.AutomationServiceMock
{
    internal interface IAutomationService
    {
        #region Methods

        Task Initialize();

        Task NotifyUserLogin(int bayId);

        #endregion Methods
    }
}
