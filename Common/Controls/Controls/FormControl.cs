using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls
{
    public static class FormControl
    {
        #region Fields

        private const string DisplayAttributeNameProperty = "Name";

        private const string DisplayAttributeResourceTypeProperty = "ResourceType";

        #endregion Fields

        #region Methods

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
                   .Warn(string.Format("Form control: cannot determine label value because no DisplayAttribute is available on the property '{0}'.", fieldName));

                return $"[{fieldName}]";
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
                   .Warn(string.Format("Form control: cannot determine label value because no resource with name '{0}' on type '{1}' is available.", name, resourceType.Name));

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
                       .Warn(string.Format("Cannot determine label value because property '{0}' is not available on model type '{1}'.", propertyName, type));

                    return null;
                }
                type = p.PropertyType;
            }

            var property = type.GetProperty(splits[splits.Length - 1]);
            if (property == null)
            {
                NLog.LogManager
                   .GetCurrentClassLogger()
                   .Warn(string.Format("Cannot determine label value because property '{0}' is not available on model type '{1}'.", fieldName, type));
            }

            return property;
        }

        #endregion Methods
    }
}
