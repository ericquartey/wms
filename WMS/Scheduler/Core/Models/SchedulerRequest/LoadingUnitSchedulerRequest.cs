using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class LoadingUnitSchedulerRequest : Model, ISchedulerRequest
    {
        #region Properties

        public int BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public int LoadingUnitId { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public int? Priority { get; set; }

        public SchedulerType SchedulerType => SchedulerType.LoadingUnit;

        public SchedulerRequestStatus Status { get; set; }

        public OperationType Type { get; } = OperationType.Withdrawal;

        #endregion

        #region Methods

        public static LoadingUnitSchedulerRequest FromLoadingUnitWithdrawalOptions(int loadingUnitId)
        {
            return new LoadingUnitSchedulerRequest
            {
                IsInstant = true,
                LoadingUnitId = loadingUnitId
            };
        }

        #endregion
    }
}
