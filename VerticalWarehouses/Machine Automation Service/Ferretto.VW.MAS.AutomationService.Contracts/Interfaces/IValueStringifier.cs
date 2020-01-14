using System.Globalization;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    public interface IValueStringifier
    {
        string Stringify(object value, CultureInfo culture);
    }

}
