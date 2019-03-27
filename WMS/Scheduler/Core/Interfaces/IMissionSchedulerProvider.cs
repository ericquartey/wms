﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IMissionSchedulerProvider :
        IUpdateAsyncProvider<Mission, int>,
        IReadAllAsyncProvider<Mission, int>
    {
        #region Methods

        Task<IEnumerable<Mission>> CreateForRequestsAsync(IEnumerable<SchedulerRequest> requests);


        #endregion
    }
}
