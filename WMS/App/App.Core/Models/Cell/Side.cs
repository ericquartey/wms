using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum Side
    {
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
