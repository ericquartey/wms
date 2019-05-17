using System.Threading.Tasks;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IEntityListViewModel : IExtensionDataEntityViewModel
    {
        #region Methods

        Task UpdateFilterTilesCountsAsync();

        #endregion
    }
}
