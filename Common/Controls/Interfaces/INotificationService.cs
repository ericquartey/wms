using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface INotificationService
    {
        #region Properties

        bool IsServiceHubConnected { get; }

        #endregion

        #region Methods

        void CheckForDataErrorConnection();

        Task EndAsync();

        Task StartAsync();

        #endregion
    }
}
