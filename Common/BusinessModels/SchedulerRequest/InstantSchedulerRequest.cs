namespace Ferretto.Common.BusinessModels
{
    public class InstantSchedulerRequest : SchedulerRequest
    {
        #region Properties

        public int? ItemId { get; set; }
        public int? LoadingUnitId { get; set; }
        public int? LoadingUnitTypeId { get; set; }
        public string Lot { get; set; }
        public int? MaterialStatusId { get; set; }
        public int? PackageTypeId { get; set; }
        public string RegistrationNumber { get; set; }
        public int? RequestedQuantity { get; set; }
        public string Sub1 { get; set; }
        public string Sub2 { get; set; }

        #endregion Properties
    }
}
