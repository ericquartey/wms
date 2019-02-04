namespace Ferretto.Common.DataModels
{
    public sealed class SchedulerRequest : ITimestamped, IDataModel
    {
        #region Properties

        public Area Area { get; set; }

        public int AreaId { get; set; }

        public Bay Bay { get; set; }

        public int? BayId { get; set; }

        public System.DateTime CreationDate { get; set; }

        public int DispatchedQuantity { get; set; }

        public int Id { get; set; }

        public bool IsInstant { get; set; }

        public Item Item { get; set; }

        public int ItemId { get; set; }

        public System.DateTime LastModificationDate { get; set; }

        public ItemList List { get; set; }

        public int? ListId { get; set; }

        public ItemListRow ListRow { get; set; }

        public int? ListRowId { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int? LoadingUnitId { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public int? LoadingUnitTypeId { get; set; }

        public string Lot { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public int? MaterialStatusId { get; set; }

        public OperationType OperationType { get; set; }

        public PackageType PackageType { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
