using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysConfgurationProvider
    {
        #region Methods

        Task LoadFromConfigurationAsync();

        #endregion
    }
}
