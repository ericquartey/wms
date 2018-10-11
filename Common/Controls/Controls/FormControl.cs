using System.ComponentModel.DataAnnotations;
using System.Linq;
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

            // locate the Display attribute
            var displayAttributeData = model.GetType()
                .GetProperty(fieldName)
                ?.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayAttribute));

            if (displayAttributeData == null)
            {
                return null;
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
                System.Diagnostics.Debug.WriteLine($"Form control: cannot determine label value because no resource with name {name} on type {resourceType.Name} is available.");
                return null;
            }

            return (string)propertyInfo.GetValue(null);
        }

        #endregion Methods
    }
}
