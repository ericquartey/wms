using System;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class LoadingUnitSchedulerRequest
    {
        #region Constructors

        public LoadingUnitSchedulerRequest()
        {
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public int Id { get; set; }

        public bool IsInstant { get; set; }

        public int LoadingUnitId { get; set; }

        public OperationType OperationType { get; } = OperationType.Pick;

        public int? Priority { get; set; }

        public SchedulerRequestStatus Status { get; set; }

        public SchedulerRequestType Type => SchedulerRequestType.LoadingUnit;

        #endregion
    }
}
