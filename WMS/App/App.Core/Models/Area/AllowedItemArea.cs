using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.AllowedItemArea))]
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
