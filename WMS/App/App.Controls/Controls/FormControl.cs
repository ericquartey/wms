using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.App.Controls.Interfaces;
using WpfScreenHelper;

namespace Ferretto.WMS.App.Controls
{
    public static class FormControl
    {
        #region Fields

        private const string DisplayAttributeNameProperty = "Name";

        private const string DisplayAttributeResourceTypeProperty = "ResourceType";

        #endregion

        #region Methods

        public static(double screenTop, double screenLeft, double screenWidth, double screenHeight) GetMainApplicationOffsetSize()
        {
            var interopHelper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            var activeScreen = Screen.FromHandle(interopHelper.Handle);
            var area = activeScreen.WorkingArea;
            var scaledFactor = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;

            var screenLeft = area.Left / scaledFactor;
            var screenTop = area.Top / scaledFactor;

            var screenWidth = area.Width / scaledFactor;
            var screenHeight = area.Height / scaledFactor;

            return (screenTop, screenLeft, screenWidth, screenHeight);
        }

        public static bool IsFieldRequired(Type type, string fieldName)
        {
            if (type == null || fieldName == null)
            {
                return false;
            }

            var property = GetProperty(type, fieldName);
            if (property == null)
            {
                return false;
            }

            // locate the Required attribute
            return property
                .CustomAttributes
                .Any(attr => attr.AttributeType == typeof(RequiredAttribute));
        }

        public static string RetrieveLocalizedFieldName(Type type, string fieldName)
        {
            if (type == null || fieldName == null)
            {
                return null;
            }

            var property = GetProperty(type, fieldName);
            if (property == null)
            {
                return $"[{fieldName}]";
            }

            // locate the Display attribute
            var displayAttributeData = property
                .CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayAttribute));

            if (displayAttributeData == null)
            {
                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Warn($"Form control: cannot determine label value because no DisplayAttribute is available on the property '{fieldName}'.");

                return null;
            }

            // get the Display attribute argument values
            var name = (string)displayAttributeData.NamedArguments.Single(
                arg => arg.MemberName == DisplayAttributeNameProperty).TypedValue.Value;
            var resourceType = displayAttributeData.NamedArguments.SingleOrDefault(
                arg => arg.MemberName == DisplayAttributeResourceTypeProperty).TypedValue.Value as System.Type;

            if (name == null || resourceType == null)
            {
                return null;
            }

            var propertyInfo = resourceType.GetProperty(name);
            if (propertyInfo == null)
            {
                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Warn($"Form control: cannot determine label value because no resource with name '{name}' on type '{resourceType.Name}' is available.");

                return $"[{name}]";
            }

            return (string)propertyInfo.GetValue(null);
        }

        public static void SetFocus(INavigableView view, string controlNameToFocus)
        {
            if (string.IsNullOrEmpty(controlNameToFocus) == false &&
                view is DependencyObject viewDep)
            {
                var elemToFocus = LayoutTreeHelper.GetVisualChildren(viewDep).OfType<FrameworkElement>()
                                                  .FirstOrDefault(item => item.Name == controlNameToFocus);
                if (elemToFocus != null)
                {
                    elemToFocus.Focus();
                }
            }
        }

        private static PropertyInfo GetProperty(Type type, string fieldName)
        {
            var splits = fieldName.Split('.');
            for (var i = 0; i < splits.Length - 1; i++)
            {
                var propertyName = splits[i];
                var p = type.GetProperty(propertyName);
                if (p == null)
                {
                    NLog.LogManager
                       .GetCurrentClassLogger()
                       .Warn($"Cannot determine label value because property '{propertyName}' is not available on model type '{type}'.");

                    return null;
                }

                type = p.PropertyType;
            }

            if (type == typeof(IModel<int>))
            {
                return null;
            }

            var property = type.GetProperty(splits[splits.Length - 1]);
            if (property == null)
            {
                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Warn($"Cannot determine label value because property '{fieldName}' is not available on model type '{type}'.");
            }

            return property;
        }

        #endregion
    }
}
