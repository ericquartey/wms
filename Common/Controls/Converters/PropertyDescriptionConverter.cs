using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public sealed class PropertyDescriptionConverter : IMultiValueConverter
    {
        #region Properties

        public Type Control { get; set; }

        public DependencyProperty Property { get; set; }

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length < 2)
            {
                throw new ArgumentException(Errors.PropertyDescriptionConverterCheckParameterError);
            }

            if (!(values[0] is FrameworkElement control))
            {
                return null;
            }

            var editControl = LayoutTreeHelper.GetVisualParents(control).OfType<FrameworkElement>().FirstOrDefault(c => c.GetType() == this.Control);
            if (this.Control == null &&
               control.TemplatedParent is ContentControl editorControlCoreTemplatedParent &&
               editorControlCoreTemplatedParent.Content is FrameworkElement editorControlCoreParent)
            {
                var editCore = LayoutTreeHelper.GetVisualChildren(editorControlCoreParent).OfType<EditorControl>()
                                                         .FirstOrDefault(e => e.DataContext is FrameworkElement);
                if (editCore == null)
                {
                    return null;
                }

                editControl = editCore.DataContext as FrameworkElement;
            }

            if (editControl == null || editControl.DataContext == null)
            {
                return null;
            }

            var bindingExpression = BindingOperations.GetBindingExpression(
                editControl,
                this.Property);

            if (bindingExpression != null)
            {
                var type = control.DataContext.GetType();
                var path = bindingExpression.ParentBinding.Path.Path;

                var localizedFieldName = FormControl.RetrieveLocalizedFieldName(type, path);

                var isFieldRequired = FormControl.IsFieldRequired(type, path);

                if (control is WmsLabel wmsLabel)
                {
                    wmsLabel.Title = localizedFieldName;
                    wmsLabel.ShowIcon(isFieldRequired);
                }

                return $"{localizedFieldName}";
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
