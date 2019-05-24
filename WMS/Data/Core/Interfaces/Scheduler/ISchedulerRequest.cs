using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
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
