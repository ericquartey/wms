using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public interface IHealthHub
    {
        #region Methods

        Task IsOnline();

        #endregion Methods
    }
}
