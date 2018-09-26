using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Scomparto
    public sealed class Compartment
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int LoadingUnitId { get; set; }
        public int CompartmentTypeId { get; set; }
        public Pairing ItemPairing { get; set; }
        public int? ItemId { get; set; }
        public string Sub1 { get; set; }
        public string Sub2 { get; set; }
        public int? MaterialStatusId { get; set; }
        public int? FifoTime { get; set; }
        public int? PackageTypeId { get; set; }
        public string Lot { get; set; }
        public string RegistrationNumber { get; set; }
        public int? MaxCapacity { get; set; }
        public int Stock { get; set; }
        public int ReservedForPick { get; set; }
        public int ReservedToStore { get; set; }
        public int? CompartmentStatusId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastHandlingDate { get; set; }
        public DateTime? InventoryDate { get; set; }
        public DateTime? FirstStoreDate { get; set; }
        public DateTime? LastStoreDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? XPosition { get; set; }
        public int? YPosition { get; set; }
        public int? LaserPointerCoordinate1 { get; set; }
        public int? LaserPointerCoordinate2 { get; set; }

        public LoadingUnit LoadingUnit { get; set; }
        public CompartmentType CompartmentType { get; set; }
        public Item Item { get; set; }
        public MaterialStatus MaterialStatus { get; set; }
        public PackageType PackageType { get; set; }
        public CompartmentStatus CompartmentStatus { get; set; }

        public List<Mission> Missions { get; set; }
    }
}
