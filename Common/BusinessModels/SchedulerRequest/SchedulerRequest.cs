using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class SchedulerRequest : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public string BayDescription { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(Scheduler.DispatchedQuantity), ResourceType = typeof(Scheduler))]
        public int DispatchedQuantity { get; set; }

        [Display(Name = nameof(Scheduler.IsInstant), ResourceType = typeof(Scheduler))]
        public bool IsInstant { get; set; }

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(General.MeasureUnit), ResourceType = typeof(General))]
        public string ItemUnitMeasure { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public string ListDescription { get; set; }

        [Display(Name = nameof(Scheduler.ItemListRow), ResourceType = typeof(Scheduler))]
        public string ListRowDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.OperationType), ResourceType = typeof(BusinessObjects))]
        public OperationType OperationType { get; set; }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public string PackageTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(General.Quantity), ResourceType = typeof(General))]
        public int RequestedQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        #endregion Properties
    }
}
