using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequest : IModel<int>
    {
        #region Properties

        bool IsInstant { get; set; }

        Enums.OperationType OperationType { get; }

        int? Priority { get; set; }

        Enums.SchedulerRequestStatus Status { get; set; }

        Enums.SchedulerRequestType Type { get; }

        #endregion
    }
}
