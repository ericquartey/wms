namespace Ferretto.WMS.Scheduler.Core
{
    public class SchedulerRequest : BusinessObject
    {
        #region Fields

        private int dispatchedQuantity;
        private int requestedQuantity;

        #endregion Fields

        #region Constructors

        public SchedulerRequest()
        {
        }

        public SchedulerRequest(SchedulerRequest request)
        {
            this.AreaId = request.AreaId;
            this.BayId = request.BayId;
            this.IsInstant = request.IsInstant;
            this.ItemId = request.ItemId;
            this.ListId = request.ListId;
            this.ListRowId = request.ListRowId;
            this.LoadingUnitId = request.LoadingUnitId;
            this.LoadingUnitTypeId = request.LoadingUnitTypeId;
            this.Lot = request.Lot;
            this.MaterialStatusId = request.MaterialStatusId;
            this.PackageTypeId = request.PackageTypeId;
            this.RegistrationNumber = request.RegistrationNumber;
            this.RequestedQuantity = request.RequestedQuantity;
            this.Sub1 = request.Sub1;
            this.Sub2 = request.Sub2;
            this.Type = request.Type;
        }

        #endregion Constructors

        #region Properties

        public int AreaId { get; set; }
        public int? BayId { get; set; }
        public System.DateTime? CreationDate { get; set; }

        public int DispatchedQuantity
        {
            get => this.dispatchedQuantity;
            set => SetIfPositive(ref this.dispatchedQuantity, value);
        }

        public bool IsInstant { get; set; }
        public int ItemId { get; set; }
        public int? ListId { get; set; }
        public int? ListRowId { get; set; }
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
    }
}
