using Ferretto.VW.App.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
{
    public static class DataAnnotationAttributeExtensions
    {
        private static string Localize<T>(this T attribute) where T: Attribute, ILocalizableString
        {
            Type resx = attribute.ResourceType;
            string resxName = attribute.ResourceName;
            if (resx != null && !string.IsNullOrEmpty(resxName))
            {
                var mngr = new System.Resources.ResourceManager(resx);
                return mngr.GetString(resxName);
            }

            return attribute.DefaultValue;
        }

        public static string Category(this CategoryAttribute attribute)
            => attribute.Localize();

        public static string Tag(this TagAttribute attribute)
            => attribute.Localize();


        public static string DisplayName (this PropertyInfo prop)
        {
            var display = prop.GetCustomAttribute<DisplayAttribute>();
            if (display != null)
            {
                return display.Name ?? display.ShortName ?? prop.Name;
            }

            return prop.Name;
        }
    }
}
