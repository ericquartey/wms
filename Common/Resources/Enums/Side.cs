using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    // Lato
    public enum Side
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.SideLeft), ResourceType = typeof(BusinessObjects))]
        Left = 'L',

        [Display(Name = nameof(BusinessObjects.SideRight), ResourceType = typeof(BusinessObjects))]
        Right = 'R',

        [Display(Name = nameof(BusinessObjects.SideFront), ResourceType = typeof(BusinessObjects))]
        Front = 'F',

        [Display(Name = nameof(BusinessObjects.SideBack), ResourceType = typeof(BusinessObjects))]
        Back = 'B',
    }
}
