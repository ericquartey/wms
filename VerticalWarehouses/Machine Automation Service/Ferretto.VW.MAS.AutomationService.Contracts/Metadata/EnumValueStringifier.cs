using System;
using System.Globalization;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class EnumValueStringifier : IValueStringifier
    {
        #region Methods

        public string Stringify(object value)
            => this.Stringify(value, CommonUtils.Culture.Actual);

        public string Stringify(object value, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (!value.GetType().IsEnum)
            {
                throw new ArgumentException("Provided argument is not an enum type.", nameof(value));
            }

            var name = Enum.GetName(value.GetType(), value);
            var resourceName = string.Concat(value.GetType().Name, ".", name);
            return Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources.Vertimag.ResourceManager.GetString(resourceName, culture) ?? name;
        }

        #endregion
    }
}
