using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface INotificationService
    {
        #region Properties

        bool IsConnected { get; }

        #endregion

        #region Methods

        Task EndAsync();

        Task StartAsync();

        #endregion
    }
}
