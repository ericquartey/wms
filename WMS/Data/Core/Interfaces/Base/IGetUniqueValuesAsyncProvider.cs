using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<object[]> GetUniqueValuesAsync(string propertyName);

        #endregion
    }
}
