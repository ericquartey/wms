using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class CompositeValidationRule : ValidationRule
    {
        public CompositeValidator CompositeValidator { get; set; }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (this.CompositeValidator?.Rules?.Count > 0)
            {
                foreach (var validator in this.CompositeValidator.Rules)
                {
                    var result = validator.Validate(value, cultureInfo);
                    if (!result.IsValid)
                    {
                        return result;
                    }
                }
            }
            return new System.Windows.Controls.ValidationResult(true, default);
        }
    }
}
