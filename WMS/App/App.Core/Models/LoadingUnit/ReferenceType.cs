using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ReferenceType
    {
        [Display(Name = nameof(BusinessObjects.MonoReference), ResourceType = typeof(BusinessObjects))]
        MonoReference = 'M',

        [Display(Name = nameof(BusinessObjects.PluriReference), ResourceType = typeof(BusinessObjects))]
        PluriReference = 'P'
    }
}
