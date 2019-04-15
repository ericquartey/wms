using System.Threading.Tasks;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IEntityListViewModel : IExtensionDataEntityViewModel
    {
        #region Properties

        bool FlattenDataSource { get; }

        #endregion

        #region Methods

        Task UpdateFilterTilesCountsAsync();

        #endregion
    }
}
