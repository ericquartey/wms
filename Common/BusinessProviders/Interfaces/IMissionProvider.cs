using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMissionProvider : IBusinessProvider<Mission, MissionDetails, int>
    {
        #region Methods

        IQueryable<Mission> GetWithStatusCompleted();

        int GetWithStatusCompletedCount();

        IQueryable<Mission> GetWithStatusNew();

        int GetWithStatusNewCount();

        #endregion
    }
}
