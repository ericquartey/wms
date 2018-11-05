using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface IBayProvider : IBusinessProvider<Bay, Bay>
    {
        #region Methods

        IQueryable<Bay> GetByAreaId(int id);

        #endregion Methods
    }
}
