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

        OperationType OperationType { get; }

        int? Priority { get; set; }

        SchedulerRequestStatus Status { get; set; }

        SchedulerRequestType Type { get; }

        #endregion
    }
}
