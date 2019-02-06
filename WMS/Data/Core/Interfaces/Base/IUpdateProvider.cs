using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IUpdateProvider<T>
    {
        #region Methods

        Task<T> UpdateAsync(T model);

        #endregion
    }
}
