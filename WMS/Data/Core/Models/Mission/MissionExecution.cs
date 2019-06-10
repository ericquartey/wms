using Ferretto.WMS.Data.Core.Interfaces.Policies;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionExecution : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public int? BayId { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public int? ItemId { get; set; }

        public int? ItemListId { get; set; }

        public int? ItemListRowId { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        [PositiveOrZero]
        public double QuantityRemainingToDispatch => this.RequestedQuantity - this.DispatchedQuantity;

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double RequestedQuantity { get; set; } // TODO: create separate models for different kinds of missions (like SchedulerRequest) and put back this chec to CheckIfStrictlyPositive

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
