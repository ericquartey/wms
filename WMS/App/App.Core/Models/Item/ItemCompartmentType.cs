using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemCompartmentType : BusinessObject
    {
        #region Fields

        private bool isActive;

        private double? maxCapacity;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.ItemCompartmentTypeCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int CompartmentsCount { get; set; }

        public int CompartmentTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCompartmentTypeEmptyCount), ResourceType = typeof(BusinessObjects))]
        public int EmptyCompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.Depth), ResourceType = typeof(BusinessObjects))]
        public double? Depth { get; set; }

        public bool IsActive
        {
            get => this.isActive;
            set => this.SetProperty(ref this.isActive, value);
        }

        public int ItemId { get; set; }

        [Display(Name = nameof(BusinessObjects.MaxCapacity), ResourceType = typeof(BusinessObjects))]
        public double? MaxCapacity
        {
            get => this.maxCapacity;
            set => this.SetProperty(ref this.maxCapacity, value);
        }

        [Display(Name = nameof(BusinessObjects.Width), ResourceType = typeof(BusinessObjects))]
        public double? Width { get; set; }

        #endregion

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                return this.GetValidationMessage(columnName);
            }
        }

        private string GetValidationMessage(string columnName)
        {
            switch (columnName)
            {
                case nameof(this.MaxCapacity):
                    return this.GetValidationMessageForMaxCapacity(columnName);
            }

            return null;
        }

        private string GetValidationMessageForMaxCapacity(string columnName)
        {
            if (this.isActive && !this.MaxCapacity.HasValue)
            {
                return this.GetErrorMessageIfZeroOrNull(null, columnName);
            }

            return this.GetErrorMessageIfNegativeOrZero(this.MaxCapacity, columnName);
        }
    }
}
