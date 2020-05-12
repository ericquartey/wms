using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.ValidationRules
{
    public class NumericValidationRule : ValidationRule
    {
        #region Properties

        public WrapperNumericValidationRule Wrapper { get; set; }

        #endregion

        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double numericValue;
            if (!double.TryParse(value?.ToString(), out numericValue))
            {
                return new ValidationResult(false, "Input value was of the wrong type, expected a numeric");
            }

            if (this.Wrapper.MinValue > numericValue)
            {
                return new ValidationResult(false, $"Minimum value {this.Wrapper.MinValue} required");
            }

            if (this.Wrapper.MaxValue < numericValue)
            {
                return new ValidationResult(false, $"Maximum value {this.Wrapper.MaxValue} required");
            }

            return new ValidationResult(true, null);
        }

        #endregion
    }
}
