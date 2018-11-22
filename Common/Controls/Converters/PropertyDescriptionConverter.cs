using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public sealed class PropertyDescriptionConverter : IMultiValueConverter
    {
        #region Properties

        public Type Control { get; set; }
        public DependencyProperty Property { get; set; }

        #endregion Properties

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
            {
                throw new ArgumentException(Errors.PropertyDescriptionConverterCheckParameterError);
            }

            if (!(values[0] is FrameworkElement control))
            {
                return null;
            }

            var editControl = LayoutTreeHelper.GetVisualParents(control).FirstOrDefault(c => c.GetType() == this.Control);
            if (((FrameworkElement)control).DataContext == null)
            {
                return null;
            }

            var bindingExpression = BindingOperations.GetBindingExpression(editControl,
                                    this.Property);

            if (bindingExpression != null)
            {
                var propertyName = bindingExpression.ParentBinding.Path.Path;
                var type = ((FrameworkElement)control).DataContext.GetType();
                var path = bindingExpression.ParentBinding.Path.Path;
                return FormControl.RetrieveLocalizedFieldName(type, path);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion Methods
    }
}
