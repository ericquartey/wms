using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public interface ISchedulerRequest : IModel<int>
    {
        #region Properties

        bool IsInstant { get; set; }

        int? Priority { get; set; }

        SchedulerType SchedulerType { get; }

        OperationType Type { get; }

        SchedulerRequestStatus Status { get; set; }

        #endregion
    }
}
