using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IGetUniqueValuesProvider
    {
        #region Methods

        Task<object[]> GetUniqueValuesAsync(string propertyName);

        #endregion Methods
    }
}
