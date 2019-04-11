using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public interface IAuthenticationService
    {
        #region Properties

        string AccessToken { get; }

        #endregion

        #region Methods

        Task<string> GetUserNameAsync();

        Task LoginAsync(string userName, string password);

        #endregion
    }
}
