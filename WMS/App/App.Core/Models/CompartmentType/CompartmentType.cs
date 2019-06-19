using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class CompartmentType : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentTypeCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int CompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeEmptyCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int EmptyCompartmentsCount { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentTypeHeightDescription), ResourceType = typeof(BusinessObjects))]
        public double? Height { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeItemCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemCompartmentsCount { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentTypeWidthDescription), ResourceType = typeof(BusinessObjects))]
        public double? Width { get; set; }

        #endregion

        #region Indexers

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

                switch (columnName)
                {
                    case nameof(this.Width):

                        return this.GetErrorMessageIfNegativeOrZero(this.Width, columnName);

                    case nameof(this.Height):

                        return this.GetErrorMessageIfNegativeOrZero(this.Height, columnName);
                }

                return null;
            }
        }

        #endregion
    }
}
