using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
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
