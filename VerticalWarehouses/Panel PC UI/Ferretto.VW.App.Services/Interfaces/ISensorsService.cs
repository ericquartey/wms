using System.Threading.Tasks;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Methods

        Task RefreshAsync();

        #endregion
    }
}
