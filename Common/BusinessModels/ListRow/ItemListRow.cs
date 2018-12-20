using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemListRow : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowDispatchedQuantity), ResourceType = typeof(BusinessObjects))]
        public int DispatchedQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusDescription), ResourceType = typeof(BusinessObjects))]
        public ItemListRowStatus ItemListRowStatus { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowRequiredQuantity), ResourceType = typeof(BusinessObjects))]
        public int RequiredQuantity { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemListRowPriority), ResourceType = typeof(BusinessObjects))]
        public int RowPriority { get; set; }

        #endregion Properties
    }
}
