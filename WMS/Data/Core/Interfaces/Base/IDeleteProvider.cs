using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IDeleteProvider
    {
        #region Methods

        Task<int> DeleteAsync(int id);

        #endregion
    }
}
