using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

            var splits = fieldName.Split('.');
            for (var i = 0; i < splits.Length - 1; i++)
            {
                type = type.GetProperty(splits[i]).PropertyType;
            }
            var property = type.GetProperty(splits[splits.Length - 1]);
            if (property == null)
            {
                System.Diagnostics.Debug.WriteLine($"Form control: cannot determine label value because property '{fieldName}' is not available on model type '{type}'.");
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

        #endregion Methods
    }
}
