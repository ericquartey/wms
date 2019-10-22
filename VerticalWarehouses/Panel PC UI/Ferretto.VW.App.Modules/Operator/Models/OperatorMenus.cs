using System.ComponentModel.DataAnnotations;

using Ferretto.VW.App.Operator.Attributes;

namespace Ferretto.VW.App.Operator.Resources
{
    public enum OperatorMenus
    {
        [View(Utils.Modules.Operator.TEST, nameof(Utils.Modules.Operator), OperatorMenuTypes.Operator)]
        [Display(Description = "TEST")]
        TEST,

        None,
    }
}
