using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services.Interfaces
{
    public interface IMachineProvider
    {
        #region Methods

        Task<MachineIdentity> GetIdentityAsync();

        #endregion
    }
}
