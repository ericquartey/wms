using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionProvider
    {
        #region Methods

        Task<IEnumerable<Mission>> GetAllAsync();

        Task<Mission> GetByIdAsync(int id);

        #endregion
    }
}
