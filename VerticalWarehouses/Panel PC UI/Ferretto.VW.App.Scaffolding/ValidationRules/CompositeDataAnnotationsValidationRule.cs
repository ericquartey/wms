using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class CompositeDataAnnotationsValidationRule : ValidationRule
    {
        public CompositeDataAnnotationValidator CompositeValidator { get; set; }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (this.CompositeValidator?.Attributes?.Count > 0)
            {
                foreach (var validator in this.CompositeValidator.Attributes)
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
