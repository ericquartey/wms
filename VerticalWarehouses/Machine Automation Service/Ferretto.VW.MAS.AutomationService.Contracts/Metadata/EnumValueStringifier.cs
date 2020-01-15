using System;
using System.Globalization;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class EnumValueStringifier : IValueStringifier
    {
        public string Stringify(object value)
            => this.Stringify(value, System.Globalization.CultureInfo.CurrentCulture);

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

            string name = Enum.GetName(value.GetType(), value);
            string resourceName = string.Concat(value.GetType().Name, ".", name);
            return Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources.Vertimag.ResourceManager.GetString(resourceName, culture) ?? name;
        }
    }
}
