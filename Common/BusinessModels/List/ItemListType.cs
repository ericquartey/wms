using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum ItemListType
    {
        [Display(Name = nameof(BusinessObjects.ItemListTypePick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.ItemListTypePut), ResourceType = typeof(BusinessObjects))]
        Put = 'U',

        [Display(Name = nameof(BusinessObjects.ItemListTypeInventory), ResourceType = typeof(BusinessObjects))]
        Inventory = 'I'
    }
}
