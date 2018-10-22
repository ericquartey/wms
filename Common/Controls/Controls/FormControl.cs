using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public class FormControl : UserControl
    {
        #region Fields

        private const string DisplayAttributeNameProperty = "Name";
        private const string DisplayAttributeResourceTypeProperty = "ResourceType";

        #endregion Fields

        #region Constructors

        protected FormControl()
        {
        }

        #endregion Constructors

        #region Methods

        protected static string RetrieveLocalizedFieldName(object model, string fieldName)
        {
            if (model == null || fieldName == null)
            {
                return null;
            }

            var property = GetProperty(model.GetType(), fieldName);
            if (property == null)
            {
                System.Diagnostics.Debug.WriteLine($"Form control: cannot determine label value because property '{fieldName}' is not available on model type '{model.GetType()}'.");
                return $"[{fieldName}]";
            }

            // locate the Display attribute
            var displayAttributeData = property
                .CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayAttribute));

            if (displayAttributeData == null)
            {
                System.Diagnostics.Debug.WriteLine($"Form control: cannot determine label value because no DisplayAttribute is available on the property '{fieldName}'.");
                return $"[{fieldName}]";
            }

            // get the Display attribute argument values
            var name = (string)displayAttributeData.NamedArguments.Single(arg => arg.MemberName == DisplayAttributeNameProperty).TypedValue.Value;
            var resourceType = displayAttributeData.NamedArguments.Single(arg => arg.MemberName == DisplayAttributeResourceTypeProperty).TypedValue.Value as System.Type;

            if (name == null || resourceType == null)
            {
                return null;
            }

            var propertyInfo = resourceType.GetProperty(name);

            if (propertyInfo == null)
            {
                System.Diagnostics.Debug.WriteLine($"Form control: cannot determine label value because no resource with name '{name}' on type '{resourceType.Name}' is available.");
                return $"[{name}]";
            }

            return (string)propertyInfo.GetValue(null);
        }

        private static PropertyInfo GetProperty(Type type, string propertyPath)
        {
            var indexOfSeparator = propertyPath.IndexOf('.');

            if (indexOfSeparator < 0)
            {
                return type.GetProperty(propertyPath);
            }

            var propertyName = propertyPath.Substring(0, indexOfSeparator);

            var propertyInfo = type.GetProperty(propertyName);

            var childPropertyPath = propertyPath.Substring(indexOfSeparator + 1);

            return GetProperty(propertyInfo.ReflectedType, childPropertyPath);
        }

        #endregion Methods
    }
}
