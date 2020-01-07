using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
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
                var metadata = entity.Metadata;
                var instance = entity.Instance;
                if (metadata != null)
                {
                    foreach (var attr in metadata)
                    {
                        if (attr is ValidationAttribute validationAttr)
                        {
                            rules.Add(new ValidationRules.AttributeValidationRule(validationAttr, instance));
                        }
                    }
                }
            }
            return new ObservableCollection<ValidationRule>(rules);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
