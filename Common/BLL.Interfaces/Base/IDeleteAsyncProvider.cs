using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IDeleteAsyncProvider
    {
        #region Methods

        Task<int> DeleteAsync(int id);

        #endregion
    }
}
