using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IEntityListViewModel
    {
        #region Methods

        void RefreshData();

        Task UpdateFilterTilesCountsAsync();

        #endregion Methods
    }
}
