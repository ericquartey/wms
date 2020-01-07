using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class AttributeValidationRule : ValidationRule
    {
        private readonly ValidationAttribute _attribute;
        private readonly object _instance;

        public AttributeValidationRule(ValidationAttribute attribute, object instance)
        {
            this._attribute = attribute;
            this._instance = instance;
        }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var result = this._attribute.GetValidationResult(value, new ValidationContext(this._instance));
            if (result == System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return new System.Windows.Controls.ValidationResult(true, default);
            }
            return new System.Windows.Controls.ValidationResult(false, result.ErrorMessage);
        }
    }
}
