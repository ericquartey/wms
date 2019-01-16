using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Ferretto.Common.Utils
{
    public static class EnumExtensions
    {
        #region Methods

        public static string GetDisplayName(this Enum enumValue, Type enumType)
        {
            if (enumType == null || enumValue == null)
            {
                return string.Empty;
            }

            var displayName = $"[{enumValue}]";
            var info = enumType.GetMember(enumValue.ToString()).First();

            if (info != null && info.CustomAttributes.Any())
            {
                var nameAttr = info.GetCustomAttribute<DisplayAttribute>();
                if (nameAttr != null && nameAttr.Name != null)
                {
                    if (nameAttr.ResourceType != null)
                    {
                        var manager = new ResourceManager(nameAttr.ResourceType);
                        displayName = manager.GetString(nameAttr.Name);
                    }
                    else
                    {
                        displayName = nameAttr.Name;
                    }
                }
            }

            return displayName;
        }

        #endregion Methods
    }
}
