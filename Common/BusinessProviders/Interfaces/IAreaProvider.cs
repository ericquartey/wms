using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IAreaProvider : IBusinessProvider<Area, Area, int>
    {
        #region Methods

        IQueryable<Area> GetByItemIdAvailability(int id);

        #endregion
    }
}
