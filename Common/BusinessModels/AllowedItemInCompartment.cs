using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class AllowedItemInCompartment : BusinessObject
    {
        #region Fields

        private int? maxCapacity;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        [Display(Name = nameof(BusinessObjects.AllowedItemInCompartmentMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public int? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.SetIfPositive(ref this.maxCapacity, value);
        }

        #endregion Properties
    }
}
