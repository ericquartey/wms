namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionExecution : BaseModel<int>
    {
        #region Fields

        private double dispatchedQuantity;

        private double requestedQuantity;

        #endregion

        #region Properties

        public int? BayId { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        public double DispatchedQuantity
        {
            get => this.dispatchedQuantity;
            set => this.dispatchedQuantity = CheckIfPositive(value);
        }

        public int? ItemId { get; set; }

        public int? ItemListId { get; set; }

        public int? ItemListRowId { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        public double QuantityRemainingToDispatch => this.RequestedQuantity - this.dispatchedQuantity;

        public string RegistrationNumber { get; set; }

        public double RequestedQuantity
        {
            get => this.requestedQuantity;
            set => this.requestedQuantity = CheckIfPositive(value); // TODO: put strictly positive
        }

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
