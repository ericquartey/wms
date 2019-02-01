using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface ICreateProvider<T>
    {
        #region Methods

        Task<T> AddAsync(T model);

        #endregion Methods
    }
}
