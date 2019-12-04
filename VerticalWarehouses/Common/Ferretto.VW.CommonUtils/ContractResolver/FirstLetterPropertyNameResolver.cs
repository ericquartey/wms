using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ferretto.VW.CommonUtils.ContractResolver
{
    public class FirstLetterPropertyNameResolver : CamelCasePropertyNamesContractResolver
    {
        #region Methods

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default:
                    return input.First()
                           .ToString()
                           .ToUpper(CultureInfo.InvariantCulture) + input.Substring(1);
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.GetCustomAttribute<JsonPropertyAttribute>() is JsonPropertyAttribute jsonProperty)
            {
                property.PropertyName = FirstCharToUpper(jsonProperty.PropertyName);
            }

            return property;
        }

        #endregion
    }
}
