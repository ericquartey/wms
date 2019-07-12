using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IGlobalSettingsProvider
    {
        #region Methods

        Task<GlobalSettings> GetAllAsync();

        #endregion
    }
}
