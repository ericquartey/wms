using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class CompartmentDetails : BusinessObject, IEntity<int>
    {
        #region Fields

        private int? fifoTime;
        private int? height;
        private int? maxCapacity;
        private int reservedForPick;
        private int reservedToStore;
        private int stock;
        private int? width;
        private int? xPosition;
        private int? yPosition;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        public IEnumerable<Enumeration<int>> CompartmentStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public int? CompartmentStatusId { get; set; }

        public IEnumerable<Enumeration<int>> CompartmentTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentType), ResourceType = typeof(BusinessObjects))]
        public int CompartmentTypeId { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentFifoTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTime
        {
            get => this.fifoTime;
            set => SetIfStrictlyPositive(ref this.fifoTime, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentFirstStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? FirstStoreDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int? Height
        {
            get => this.height;
            set => SetIfStrictlyPositive(ref this.height, value);
        }

        public int Id { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCode_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemCode { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentPairing), ResourceType = typeof(BusinessObjects))]
        public DataModels.Pairing ItemPairing { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentHandlingDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastHandlingDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLastStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastStoreDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode_extended), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        public IEnumerable<Enumeration<int>> MaterialStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public int? MaterialStatusId { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => SetIfStrictlyPositive(ref this.maxCapacity, value);
        }

        public IEnumerable<Enumeration<int>> PackageTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public int? PackageTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedForPick), ResourceType = typeof(BusinessObjects))]
        public int ReservedForPick
        {
            get => this.reservedForPick;
            set => SetIfPositive(ref this.reservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedToStore), ResourceType = typeof(BusinessObjects))]
        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => SetIfPositive(ref this.reservedToStore, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set => SetIfPositive(ref this.stock, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public int? Width
        {
            get => this.width;
            set => SetIfStrictlyPositive(ref this.width, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int? XPosition
        {
            get => this.xPosition;
            set => SetIfPositive(ref this.xPosition, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int? YPosition
        {
            get => this.yPosition;
            set => SetIfPositive(ref this.yPosition, value);
        }

        #endregion Properties
    }
}
