using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMissionProvider : IBusinessProvider<Mission, MissionDetails>
    {
        #region Methods

        IQueryable<Mission> GetWithStatusCompleted();

        int GetWithStatusCompletedCount();

        IQueryable<Mission> GetWithStatusNew();

        int GetWithStatusNewCount();

        #endregion Methods
    }
}
