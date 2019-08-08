using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.App.Core.Models
{
    public class MissionOperation : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(Resources.Scheduler.CompartmentType), ResourceType = typeof(Resources.Scheduler))]
        public string CompartmentType { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.DispatchedQuantity), ResourceType = typeof(BusinessObjects))]
        public double DispatchedQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public string ItemListDescription { get; set; }

        [Display(Name = nameof(ItemLists.ItemListRow), ResourceType = typeof(ItemLists))]
        public string ItemListRowDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.MeasureUnitDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemMeasureUnitDescription { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode { get; set; }

        [Display(Name = nameof(BusinessObjects.Lot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        public string MeasureUnitDescription { get; set; }

        public int MissionId { get; set; }

        public IEnumerable<MissionOperation> Operations { get; set; }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public string PackageTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.Priority), ResourceType = typeof(BusinessObjects))]
        public int? Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.Quantity), ResourceType = typeof(BusinessObjects))]
        public double RequestedQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.Status), ResourceType = typeof(BusinessObjects))]
        public Enums.MissionOperationStatus Status { get; set; }

        [Display(Name = nameof(BusinessObjects.Sub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.Sub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        [Display(Name = nameof(BusinessObjects.Type), ResourceType = typeof(BusinessObjects))]
        public Enums.MissionOperationType Type { get; set; }

        #endregion
    }
}
