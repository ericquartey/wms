using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum ReferenceType
    {
        [Display(Name = nameof(BusinessObjects.MonoReference), ResourceType = typeof(BusinessObjects))]
        MonoReference = 'M',

        [Display(Name = nameof(BusinessObjects.PluriReference), ResourceType = typeof(BusinessObjects))]
        PluriReference = 'P'
    }
}
