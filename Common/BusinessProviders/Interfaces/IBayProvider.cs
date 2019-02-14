using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IBayProvider : IBusinessProvider<Bay, Bay, int>
    {
        #region Methods

        IQueryable<Bay> GetByAreaId(int id);

        #endregion
    }
}
