using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(SchedulerRequest))]
    public class LoadingUnitSchedulerRequest : BaseModel<int>, ISchedulerRequest
    {
        #region Properties

        public int BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public int LoadingUnitId { get; set; }

        public Enums.OperationType OperationType { get; } = Enums.OperationType.Pick;

        public int? Priority { get; set; }

        public Enums.SchedulerRequestStatus Status { get; set; }

        public Enums.SchedulerRequestType Type => Enums.SchedulerRequestType.LoadingUnit;

        #endregion

        #region Methods

        public static LoadingUnitSchedulerRequest FromLoadingUnitWithdrawalOptions(int loadingUnitId)
        {
            return new LoadingUnitSchedulerRequest
            {
                IsInstant = true,
                LoadingUnitId = loadingUnitId,
            };
        }

        #endregion
    }
}
