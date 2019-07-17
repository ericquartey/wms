using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ItemListType
    {
        [Display(Name = nameof(BusinessObjects.Pick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.Put), ResourceType = typeof(BusinessObjects))]
        Put = 'U',

        [Display(Name = nameof(BusinessObjects.Inventory), ResourceType = typeof(BusinessObjects))]
        Inventory = 'I',
    }
}
