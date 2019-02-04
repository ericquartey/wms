using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class Mission : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public string BayDescription { get; set; }

        [Display(Name = nameof(MasterData.CellInformation), ResourceType = typeof(MasterData))]
        public string CellDescription { get; set; }

        [Display(Name = nameof(Scheduler.CompartmentType), ResourceType = typeof(Scheduler))]
        public string CompartmentType { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public string ItemListDescription { get; set; }

        [Display(Name = nameof(MasterData.ItemListRows), ResourceType = typeof(MasterData))]
        public string ItemListRowDescription { get; set; }

        public string ItemUnitMeasure { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public string PackageTypeDescription { get; set; }

        [Display(Name = nameof(General.Priority), ResourceType = typeof(General))]
        public int? Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(General.Quantity), ResourceType = typeof(General))]
        public int RequiredQuantity { get; set; }

        [Display(Name = nameof(General.Status), ResourceType = typeof(General))]
        public MissionStatus Status { get; set; } = MissionStatus.New;

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public MissionType Type { get; set; }

        #endregion
    }
}
