using System;

namespace Ferretto.Common.DataModels
{
    // Missione
    public sealed class Mission : IDataModel, ITimestamped
    {
        #region Properties

        public Bay Bay { get; set; }

        public int? BayId { get; set; }

        public Cell Cell { get; set; }

        public int? CellId { get; set; }

        public Compartment Compartment { get; set; }

        public int? CompartmentId { get; set; }

        public DateTime CreationDate { get; set; }

        public int Id { get; set; }

        public Item Item { get; set; }

        public int? ItemId { get; set; }

        public ItemList ItemList { get; set; }

        public int? ItemListId { get; set; }

        public ItemListRow ItemListRow { get; set; }

        public int? ItemListRowId { get; set; }

        public DateTime LastModificationDate { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public int? MaterialStatusId { get; set; }

        public PackageType PackageType { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequiredQuantity { get; set; }

        public MissionStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion Properties
    }
}
