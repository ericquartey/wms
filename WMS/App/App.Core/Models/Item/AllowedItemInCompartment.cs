﻿using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class AllowedItemInCompartment : BusinessObject
    {
        #region Fields

        private double? maxCapacity;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemImage), ResourceType = typeof(BusinessObjects))]
        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCategory), ResourceType = typeof(BusinessObjects))]
        public string ItemCategoryDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.AllowedItemInCompartmentMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public double? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.SetProperty(ref this.maxCapacity, value);
        }

        #endregion
    }
}
