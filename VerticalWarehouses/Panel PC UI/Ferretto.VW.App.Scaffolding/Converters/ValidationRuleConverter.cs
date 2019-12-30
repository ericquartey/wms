using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class ValidationRuleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rules = new List<ValidationRule>();
            if (value is Models.ScaffoldedEntity entity)
            {
                var prop = entity.Property;
                var instance = entity.Instance;
                foreach (var validationAttr in prop.GetCustomAttributes<ValidationAttribute>())
                {
                    rules.Add(new ValidationRules.AttributeValidationRule(validationAttr, instance));
                }
            }
            return rules;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
