namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class Mission : Model
    {
        #region Fields

        private int requestedQuantity;

        #endregion

        #region Properties

        public int? BayId { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        public int? ItemId { get; set; }

        public int? ItemListId { get; set; }

        public int? ItemListRowId { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        public int RequestedQuantity
        {
            get => this.requestedQuantity;
            set => SetIfStrictlyPositive(ref this.requestedQuantity, value);
        }
        {
            get => this.quantity;
            set => SetIfStrictlyPositive(ref this.quantity, value);
        }

        public string RegistrationNumber { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
