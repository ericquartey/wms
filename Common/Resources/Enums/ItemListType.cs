using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    // Tipo Lista Articoli
    public enum ItemListType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.Pick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.Put), ResourceType = typeof(BusinessObjects))]
        Put = 'U',

        [Display(Name = nameof(BusinessObjects.Inventory), ResourceType = typeof(BusinessObjects))]
        Inventory = 'I',
    }
}
