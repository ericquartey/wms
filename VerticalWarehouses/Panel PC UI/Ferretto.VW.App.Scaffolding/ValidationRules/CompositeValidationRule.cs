using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{

    public class CompositeValidationRule : NotifyValidationRule
    {
        public CompositeValidator CompositeValidator { get; set; }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var compositeValidator = this.CompositeValidator;
            if (compositeValidator?.Rules?.Count > 0)
            {
                foreach (var validator in compositeValidator.Rules)
                {
                    var result = validator.Validate(value, cultureInfo);
                    if (!result.IsValid)
                    {
                        this.OnValidated(new ValidationEventArgs(false, result.ErrorContent));
                        return result;
                    }
                }
            }
            this.OnValidated(new ValidationEventArgs(true));
            return new System.Windows.Controls.ValidationResult(true, default);
        }

    }
}
