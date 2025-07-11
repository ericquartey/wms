﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Scaffolding.Models;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    public static class DataAnnotationAttributeExtensions
    {
        #region Methods

        public static string Category(this CategoryAttribute attribute)
            => (attribute ?? throw new ArgumentNullException(nameof(attribute))).Localize();

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

        public static object DefaultValue(this ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.DefaultValue();

        public static string Description(this CategoryDescriptionAttribute attribute)
            => (attribute ?? throw new ArgumentNullException(nameof(attribute))).Localize();

        public static string DisplayName(this ScaffoldedEntity entity)
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
                string dn = AutomationService.Contracts.Metadata.Resources.Vertimag.ResourceManager.GetString(display.Name, Localized.Instance.CurrentCulture);

                return dn ?? display.GetName() ?? display.GetShortName();
            }

            return property.Name;
        }

        public static IEnumerable<ValidationRule> ExtractValidationRules(this ScaffoldedEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var metadata = entity.Metadata;
            var instance = entity.Instance;
            if (metadata != null)
            {
                foreach (var attr in metadata)
                {
                    if (attr is ValidationAttribute validationAttr)
                    {
                        yield return new App.Scaffolding.ValidationRules.AttributeValidationRule(
                            validationAttr,
                            instance,
                            entity.Property.DisplayName(metadata));
                    }
                }
            }
        }

        public static bool IsEditable(this ScaffoldedEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            return entity.Property.IsEditable(entity.Metadata);
        }

        public static bool IsEditable(this PropertyInfo property, IEnumerable<Attribute> metadata)
            => property.IsEditable(metadata.OfType<EditableAttribute>().FirstOrDefault());

        public static bool IsEditable(this PropertyInfo property, EditableAttribute editable)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (editable != null)
            {
                return editable.AllowEdit;
            }

            return true;
        }

        public static object RangeMax(this IEnumerable<Attribute> metadata)
            => (metadata ?? throw new ArgumentNullException(nameof(metadata))).OfType<RangeAttribute>().FirstOrDefault()?.Maximum;

        public static object RangeMax(this ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.RangeMax();

        public static object RangeMin(this IEnumerable<Attribute> metadata)
            => (metadata ?? throw new ArgumentNullException(nameof(metadata))).OfType<RangeAttribute>().FirstOrDefault()?.Minimum;

        public static object RangeMin(this ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.RangeMin();

        public static string Tag(this TagAttribute attribute)
            => (attribute ?? throw new ArgumentNullException(nameof(attribute))).Localize();

        public static string UnitOfMeasure(this IEnumerable<Attribute> metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }
            var defaultAttr = metadata.OfType<UnitAttribute>().FirstOrDefault();
            if (defaultAttr != null)
            {
                return defaultAttr.Unit;
            }
            return default;
        }

        public static string UnitOfMeasure(this ScaffoldedEntity entity)
            => (entity ?? throw new ArgumentNullException(nameof(entity))).Metadata?.UnitOfMeasure();

        private static string Localize<T>(this T attribute) where T : Attribute, ILocalizableString
        {
            Type resx = attribute.ResourceType;
            string resxName = attribute.ResourceName;
            if (resx != null && !string.IsNullOrEmpty(resxName))
            {
                var mngr = new System.Resources.ResourceManager(resx);
                return mngr.GetString(resxName, Localized.Instance.CurrentCulture);
            }

            return attribute.DefaultValue;
        }

        #endregion
    }
}
