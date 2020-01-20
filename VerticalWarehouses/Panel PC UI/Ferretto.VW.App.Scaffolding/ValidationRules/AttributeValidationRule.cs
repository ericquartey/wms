using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class AttributeValidationRule : ValidationRule
    {
        private readonly ValidationAttribute _attribute;
        private readonly ValidationContext _context;

        public AttributeValidationRule(ValidationAttribute attribute, object instance, string displayName)
        {
            this._attribute = attribute;
            this._context = new ValidationContext(instance)
            {
                DisplayName = displayName
            };
        }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var result = this._attribute.GetValidationResult(value, this._context);
            if (result == System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return new System.Windows.Controls.ValidationResult(true, default);
            }
            return new System.Windows.Controls.ValidationResult(false, result.ErrorMessage);
        }
    }
}
