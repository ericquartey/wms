using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class SchedulerRequest : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public string BayDescription { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(Scheduler.IsInstant), ResourceType = typeof(Scheduler))]
        public bool IsInstant { get; set; }

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public string ListDescription { get; set; }

        [Display(Name = nameof(Scheduler.ItemListRow), ResourceType = typeof(Scheduler))]
        public string ListRowCode { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.Lot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(General.MeasureUnit), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.OperationType), ResourceType = typeof(BusinessObjects))]
        public OperationType? OperationType { get; set; }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public string PackageTypeDescription { get; set; }

        public int? Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(General.Quantity), ResourceType = typeof(General))]
        public double? RequestedQuantity { get; set; }

        [Display(Name = nameof(Scheduler.ReservedQuantity), ResourceType = typeof(Scheduler))]
        public double? ReservedQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.Status), ResourceType = typeof(BusinessObjects))]
        public SchedulerRequestStatus? Status { get; set; }

        [Display(Name = nameof(BusinessObjects.Sub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.Sub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        [Display(Name = nameof(BusinessObjects.Type), ResourceType = typeof(BusinessObjects))]
        public SchedulerRequestType? Type { get; set; }

        #endregion
    }
}
