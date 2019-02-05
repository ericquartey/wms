using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IEntityListViewModel : IRefreshDataEntityViewModel
    {
        #region Properties

        bool FlattenDataSource { get; }

        #endregion

        #region Methods

        Task UpdateFilterTilesCountsAsync();

        #endregion
    }
}
