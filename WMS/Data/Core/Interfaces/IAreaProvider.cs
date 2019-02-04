using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAreaProvider :
        IReadAllProvider<Area>,
        IReadSingleProvider<Area, int>
    {
        #region Methods

        Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id);

        #endregion
    }
}
