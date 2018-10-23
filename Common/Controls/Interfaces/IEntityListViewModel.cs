using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IEntityListViewModel
    {
        #region Properties

        bool FlattenDataSource { get; }

        #endregion Properties

        #region Methods

        void RefreshData();

        Task UpdateFilterTilesCountsAsync();

        #endregion Methods
    }
}
