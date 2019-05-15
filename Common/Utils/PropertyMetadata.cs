using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Ferretto.Common.Utils
{
    public static class PropertyMetadata
    {
        #region Fields

        private const string DisplayAttributeNameProperty = "Name";

        private const string DisplayAttributeResourceTypeProperty = "ResourceType";

        #endregion

        #region Methods

        public static bool IsFieldRequired(Type type, string fieldPathName)
        {
            if (type == null || fieldPathName == null)
            {
                return false;
            }

            var property = GetProperty(type, fieldPathName);
            if (property == null)
            {
                return false;
            }

            // locate the Required attribute
            return property
                .CustomAttributes
                .Any(attr => attr.AttributeType == typeof(RequiredAttribute));
        }

        public static string LocalizeFieldName(Type type, string fieldPathName)
        {
            if (type == null || fieldPathName == null)
            {
                return null;
            }

            var property = GetProperty(type, fieldPathName);
            if (property == null)
            {
                return $"[{fieldPathName}]";
            }

            // locate the Display attribute
            var displayAttributeData = property
                .CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayAttribute));

            if (displayAttributeData == null)
            {
                System.Diagnostics.Debug.Print(
                    $"Form control: cannot determine label value because no DisplayAttribute is available on the property '{fieldPathName}'.");

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
                System.Diagnostics.Debug.Print(
                       $"Form control: cannot determine label value because no resource with name '{name}' on type '{resourceType.Name}' is available.");

                return $"[{name}]";
            }

            return (string)propertyInfo.GetValue(null);
        }

        private static PropertyInfo GetProperty(Type type, string fieldPathName)
        {
            var pathTokens = fieldPathName.Split('.');
            if (pathTokens.Length == 1)
            {
                return null;
            }

            for (var i = 0; i < pathTokens.Length - 1; i++)
            {
                var propertyName = pathTokens[i];
                var p = type.GetProperty(propertyName);
                if (p == null)
                {
                    System.Diagnostics.Debug.Print(
                        $"Cannot determine label value because property '{propertyName}' is not available on model type '{type}'.");

                    return null;
                }

                type = p.PropertyType;
            }

            var property = type.GetProperty(pathTokens[pathTokens.Length - 1]);
            if (property == null)
            {
                System.Diagnostics.Debug.Print(
                    $"Cannot determine label value because property '{fieldPathName}' is not available on model type '{type}'.");
            }

            return property;
        }

        #endregion
    }
}
