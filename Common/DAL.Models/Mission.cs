using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Missione
    public sealed class Mission
    {
        public int Id { get; set; }
        public string MissionTypeId { get; set; }
        public string MissionStatusId { get; set; }
        public int? SourceCellId { get; set; }
        public int? DestinationCellId { get; set; }
        public int? SourceBayId { get; set; }
        public int? DestinationBayId { get; set; }
        public int? LoadingUnitId { get; set; }
        public int? CompartmentId { get; set; }
        public int? ItemListId { get; set; }
        public int? ItemListRowId { get; set; }
        public int? ItemId { get; set; }
        public string Sub1 { get; set; }
        public string Sub2 { get; set; }
        public int? MaterialStatusId { get; set; }
        public int? PackageTypeId { get; set; }
        public string Lot { get; set; }
        public string RegistrationNumber { get; set; }
        public int RequiredQuantity { get; set; }
        public int Priority { get; set; }

        public MissionType MissionType { get; set; }
        public MissionStatus MissionStatus { get; set; }
        public Cell SourceCell { get; set; }
        public Cell DestinationCell { get; set; }
        public Bay SourceBay { get; set; }
        public Bay DestinationBay { get; set; }
        public LoadingUnit LoadingUnit { get; set; }
        public Compartment Compartment { get; set; }
        public ItemList ItemList { get; set; }
        public ItemListRow ItemListRow { get; set; }
        public Item Item { get; set; }
        public MaterialStatus MaterialStatus { get; set; }
        public PackageType PackageType { get; set; }
    }
}
