using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.CompartmentType))]
    public sealed class CompartmentType : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentTypeCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int CompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeEmptyCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int EmptyCompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeHeightDescription), ResourceType = typeof(BusinessObjects))]
        public double? Height { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeItemCompartmentsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemCompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentTypeWidthDescription), ResourceType = typeof(BusinessObjects))]
        public double? Width { get; set; }

        #endregion
    }
}
