using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Hubs
{
    public interface IHealthHub
    {
        #region Methods

        Task IsOnlineAsync();

        #endregion
    }
}
