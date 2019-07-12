using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IGlobalSettingsProvider
    {
        #region Methods

        Task<GlobalSettings> GetGlobalSettingsAsync();

        #endregion
    }
}
