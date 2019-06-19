using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IRefreshableDataSource
    {
        #region Methods

        Task RefreshAsync();

        #endregion
    }
}
