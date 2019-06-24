using System;

namespace Ferretto.Common.DataModels
{
    public sealed class MissionOperation : IDataModel, ITimestamped
    {
        #region Properties

        public Compartment Compartment { get; set; }

        public int CompartmentId { get; set; }

        public DateTime CreationDate { get; set; }

        public double DispatchedQuantity { get; set; }

        public int Id { get; set; }

        public Item Item { get; set; }

        public int ItemId { get; set; }

        public ItemList ItemList { get; set; }

        public int? ItemListId { get; set; }

        public ItemListRow ItemListRow { get; set; }

        public int? ItemListRowId { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string Lot { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public int? MaterialStatusId { get; set; }

        public Mission Mission { get; set; }

        public int MissionId { get; set; }

        public PackageType PackageType { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        public double RequestedQuantity { get; set; }

        public MissionOperationStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionOperationType Type { get; set; }

        #endregion
    }
}
