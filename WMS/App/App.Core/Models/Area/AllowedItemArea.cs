using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class AllowedItemArea : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.AllowedItemAreaName), ResourceType = typeof(BusinessObjects))]
        public string Name { get; set; }

        [Display(Name = nameof(BusinessObjects.AllowedItemAreaTotalStock), ResourceType = typeof(BusinessObjects))]
        public double TotalStock { get; set; }

        #endregion
    }
}
