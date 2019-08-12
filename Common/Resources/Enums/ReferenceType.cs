using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    // Tipo Referenza
    public enum ReferenceType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MonoReference), ResourceType = typeof(BusinessObjects))]
        MonoReference = 'M',

        [Display(Name = nameof(BusinessObjects.PluriReference), ResourceType = typeof(BusinessObjects))]
        PluriReference = 'P',
    }
}
