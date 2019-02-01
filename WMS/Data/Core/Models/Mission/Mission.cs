namespace Ferretto.WMS.Data.Core.Models
{
    public class Mission : BaseModel<int>
    {
        #region Fields

        private int quantity;

        #endregion Fields

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

        public int Quantity
        {
            get => this.quantity;
            set => this.quantity = CheckIfStrictlyPositive(value);
        }

        public string RegistrationNumber { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion Properties
    }
}
