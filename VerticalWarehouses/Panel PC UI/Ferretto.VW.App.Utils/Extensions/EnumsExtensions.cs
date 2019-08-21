using System;
using System.Linq;

namespace Ferretto.VW.Utils
{
    public static class EnumExtensions
    {
        #region Methods

        public static T GetAttributeOfType<TEnum, T>(this TEnum value)
              where TEnum : struct, IConvertible
              where T : Attribute
        {
            return value.GetType()
                        .GetMember(value.ToString())
                        .First()
                        .GetCustomAttributes(false)
                        .OfType<T>()
                        .LastOrDefault();
        }

        #endregion
    }
}
