using Ferretto.VW.App.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
{
    public static class DataAnnotationAttributeExtensions
    {
        #region Localization

        private static string Localize<T>(this T attribute) where T: Attribute, ILocalizableString
        {
            Type resx = attribute.ResourceType;
            string resxName = attribute.ResourceName;
            if (resx != null && !string.IsNullOrEmpty(resxName))
            {
                var mngr = new System.Resources.ResourceManager(resx);
                return mngr.GetString(resxName, Thread.CurrentThread.CurrentUICulture);
            }

            return attribute.DefaultValue;
        }

        public static string Category(this CategoryAttribute attribute)
            => (attribute ?? throw new ArgumentNullException(nameof(attribute))).Localize();

        public static string Tag(this TagAttribute attribute)
            => (attribute ?? throw new ArgumentNullException(nameof(attribute))).Localize();

        #endregion

        #region Display name

        public static string DisplayName(this Models.ScaffoldedEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            return entity.Property.DisplayName(entity.Metadata);
        }

        public static string DisplayName(this PropertyInfo property, IEnumerable<Attribute> metadata)
            => property.DisplayName(metadata.OfType<DisplayAttribute>().FirstOrDefault());

        public static string DisplayName(this PropertyInfo property, DisplayAttribute display)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (display == null)
            {
                display = property.GetCustomAttribute<DisplayAttribute>();
            }
            if (display != null)
            {
                return display.Name ?? display.ShortName;
            }

            return property.Name;
        }

        #endregion

        #region Default value/unit/Range min/max

        public static object DefaultValue(this IEnumerable<Attribute> metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }
            var defaultAttr = metadata.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (defaultAttr != null)
            {
                return defaultAttr.Value;
            }
            return null;
        }

        public static object DefaultValue(this Models.ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.DefaultValue();

        public static string UnitOfMeasure(this IEnumerable<Attribute> metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }
            var defaultAttr = metadata.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (defaultAttr != null)
            {
                return defaultAttr.Unit;
            }
            return default;
        }

        public static string UnitOfMeasure(this Models.ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.UnitOfMeasure();

        public static object RangeMin(this IEnumerable<Attribute> metadata)
            => (metadata ?? throw new ArgumentNullException(nameof(metadata))).OfType<RangeAttribute>().FirstOrDefault()?.Minimum;

        public static object RangeMin(this Models.ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.RangeMin();

        public static object RangeMax(this IEnumerable<Attribute> metadata)
            => (metadata ?? throw new ArgumentNullException(nameof(metadata))).OfType<RangeAttribute>().FirstOrDefault()?.Maximum;

        public static object RangeMax(this Models.ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.RangeMax();

        #endregion
    }
}
