namespace Ferretto.Common.BusinessModels
{
    public class SchedulerRequest : BusinessObject
    {
        #region Fields

        private int requestedQuantity;

        #endregion Fields

        #region Properties

        public int? AreaId { get; set; }
        public int? BayId { get; set; }
        public System.DateTime? CreationDate { get; set; }
        public bool IsInstant { get; set; }
        public int ItemId { get; set; }
        public int? LoadingUnitId { get; set; }
        public int? LoadingUnitTypeId { get; set; }
        public string Lot { get; set; }
        public int? MaterialStatusId { get; set; }
        public int? PackageTypeId { get; set; }
        public string RegistrationNumber { get; set; }

        public int RequestedQuantity
        {
            get => this.requestedQuantity;
            set => this.SetIfStrictlyPositive(ref this.requestedQuantity, value);
        }

        public string Sub1 { get; set; }
        public string Sub2 { get; set; }
        public OperationType Type { get; set; }

        #endregion Properties

        // public ItemList List { get; set; }
        // public ItemListRow ListRow { get; set; }

        #region Methods

        public SchedulerRequest Clone()
        {
            return (SchedulerRequest)this.MemberwiseClone();
        }

        #endregion Methods
    }
}
