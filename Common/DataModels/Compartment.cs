﻿using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    public sealed class Compartment : ITimestamped, IDataModel
    {
        #region Properties

        public CompartmentStatus CompartmentStatus { get; set; }

        public int? CompartmentStatusId { get; set; }

        public CompartmentType CompartmentType { get; set; }

        public int CompartmentTypeId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? FifoStartDate { get; set; }

        public bool HasRotation { get; set; }

        public int Id { get; set; }

        public DateTime? InventoryDate { get; set; }

        public bool IsItemPairingFixed { get; set; }

        public Item Item { get; set; }

        public int? ItemId { get; set; }

        public DateTime LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public int? MaterialStatusId { get; set; }

        public List<Mission> Missions { get; set; }

        public PackageType PackageType { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public double ReservedForPick { get; set; }

        public double ReservedToStore { get; set; }

        public double Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public double XPosition { get; set; }

        public double YPosition { get; set; }

        #endregion
    }
}
