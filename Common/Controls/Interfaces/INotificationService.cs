using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface INotificationServiceClient
    {
        #region Methods

        Task EndAsync();

        Task StartAsync();

        #endregion Methods
    }
}
