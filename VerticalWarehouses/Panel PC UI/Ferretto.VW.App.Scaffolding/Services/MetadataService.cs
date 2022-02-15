using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ferretto.VW.App.Scaffolding.Exceptions;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Services
{
    public static class MetadataService
    {
        #region Methods

        public static Models.ScaffoldedStructure Scaffold(this object instance)
            => instance.Scaffold(Ferretto.VW.App.Resources.Localized.Instance.CurrentCulture);

        public static Models.ScaffoldedStructure Scaffold(this object instance, CultureInfo culture)
            => new MetadataServiceExecutor(culture).ScaffoldTypeInternal(instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)), instance, new ScaffoldedStructureInternal());

        public static bool TryGetCustomAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute
        {
            attribute = (member ?? throw new ArgumentNullException(nameof(member))).GetCustomAttribute<T>();
            return attribute != null;
        }

        public static bool TryGetCustomAttributes<T>(this MemberInfo member, out T[] attributes) where T : Attribute
        {
            attributes = (member ?? throw new ArgumentNullException(nameof(member))).GetCustomAttributes<T>().ToArray();
            return attributes != null;
        }

        private static Models.ScaffoldedEntity Publish(this ScaffoldedEntityInternal entity)
            => new Models.ScaffoldedEntity(entity.Property, entity.Instance, entity.Metadata, entity.Id);

        private static Models.ScaffoldedStructure Publish(this ScaffoldedStructureInternal tree)
        {
            return new Models.ScaffoldedStructure(tree.Category,
                tree.Entities
                    .Where(e => e.Property != null && e.Instance != null)
                    .Select(e => e.Publish())
                    .OrderBy(o => o.Id),
                tree.Children
                    .Select(c => c.Publish())
                    .Where(c => c.Entities.Any() || c.Children.Any())
                    .OrderBy(o => o.Id)
                )
            {
                Description = tree.Description,
                Id = tree.Id,
            };
        }

        #endregion

        #region Classes

        /// <summary>
        /// Use it and throw it away...
        /// </summary>
        private class MetadataServiceExecutor
        {
            #region Fields

            private readonly CultureInfo _culture;

            #endregion

            //private int idSeed = 0;

            #region Constructors

            public MetadataServiceExecutor(CultureInfo culture)
            {
                this._culture = culture;
            }

            #endregion

            #region Methods

            public Models.ScaffoldedStructure ScaffoldTypeInternal(Type type, object instance, ScaffoldedStructureInternal branch, ScaffoldedStructureInternal root = default, MemberInfo porpParent = default, bool unfoldingBranch = false)
            {
                root = root ?? branch;
                if (instance != null && instance.GetType() != type)
                {
                    throw new ArgumentException($"Type mismatch: '{ instance.GetType() }' does not match '{ type }'.");
                }
                var modelType = type.GetCustomAttribute<Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataTypeAttribute>()?.MetadataClassType ?? type;
                var dict = new Dictionary<string, List<Type>>();

                foreach (var prop in GetMemberInfos(modelType).Where(x => !branch.ExclusionList?.Contains(x.Name) ?? true))
                {
                    var actualProp = type.GetProperty(prop.Name);
                    if (actualProp is null)
                    {
                        // configuration error
                        continue;
                    }
                    var propertyType = prop.MemberType == MemberTypes.Field ? ((FieldInfo)prop).FieldType : ((PropertyInfo)prop).PropertyType;
                    var target = branch;

                    // skip non-scaffoldable
                    if (prop.TryGetCustomAttribute<ScaffoldColumnAttribute>(out var scaffold) && !scaffold.Scaffold)
                    {
                        continue;
                    }

                    // pull to root?
                    if (prop.TryGetCustomAttribute<PullToRootAttribute>(out var _))
                    {
                        target = root;
                        // breaks unfolding branch propagation!
                        unfoldingBranch = false;
                    }

                    // categorization?
                    var hasCategory = prop.TryGetCustomAttribute<CategoryAttribute>(out var categoryAttr);
                    var categoryParameters = prop.GetCustomAttributes<CategoryParameterAttribute>();
                    var hasCategoryParameters = categoryParameters.Any();
                    var isSimpleType = IsSimpleType(propertyType);

                    var isBay = hasCategory && hasCategoryParameters && prop.Name == "Bays" && categoryParameters.Any(p => p.PropertyReference == "Number");

                    var id = 0;
                    if (prop.TryGetCustomAttribute<IdAttribute>(out var idAttribute))
                    {
                        id = idAttribute.Id;
                    }

                    var offset = 0;
                    if (prop.TryGetCustomAttribute<OffsetAttribute>(out var offsetAttribute))
                    {
                        offset = offsetAttribute.Offset;
                    }

                    var exclusionList = new List<string>();
                    if (prop.TryGetCustomAttribute<HidePropertiesAttribute>(out var hidePropertiesAttribute))
                    {
                        exclusionList = hidePropertiesAttribute.PropertyList;
                    }

                    prop.TryGetCustomAttributes<FilterPropertiesAttribute>(out var filterAttributes);

                    #region array? (must have Category AND CategoryParameter attributes)

                    // flattening
                    if (!isSimpleType && (typeof(System.Collections.IEnumerable)).IsAssignableFrom(propertyType))
                    {
                        if (!hasCategory)
                        {
                            throw new ScaffoldingException($"Cannot find a {nameof(CategoryAttribute)} on the enumerable property {prop.Name} ({modelType}).");
                        }
                        if (!hasCategoryParameters)
                        {
                            throw new ScaffoldingException($"Cannot find at least one {nameof(CategoryParameterAttribute)} on the enumerable property {prop.Name} ({modelType}).");
                        }
                        var elementType = propertyType.GetElementType() ?? propertyType.GetGenericArguments().Single();

                        if (instance == null)
                        {
                            // cannot retrieve flattening properties from a null instance, skipping...
                            continue;
                        }

                        if (actualProp.GetValue(instance) is System.Collections.IEnumerable collection)
                        {
                            var format = categoryAttr.Category();
                            foreach (var item in collection)
                            {
                                if (item == null)
                                {
                                    // there might be missing/null items
                                    continue;
                                }
                                if (isBay && item is MAS.AutomationService.Contracts.Bay bay
                                    && bay?.Number == MAS.AutomationService.Contracts.BayNumber.ElevatorBay)
                                {
                                    continue;
                                }
                                var category = GetCategoryName(elementType, format, item, this._culture, categoryParameters.ToArray());
                                var categoryDescription = GetCategoryDescription(prop);
                                var newBranch = target.Children.FirstOrDefault(b => b.Category == category.Name);
                                if (newBranch != null)
                                {
                                    throw new ScaffoldingException($"A category with the name {category.Name} already exists.");
                                }

                                // find to index
                                var index = 0;
                                foreach (var i in collection)
                                {
                                    if (item.Equals(i))
                                    {
                                        break;
                                    }

                                    index++;
                                }

                                newBranch = new ScaffoldedStructureInternal
                                {
                                    Category = category.Name,
                                    Parent = target,
                                    Description = categoryDescription,
                                    ExclusionList = filterAttributes.SelectMany(x => x.GetExclusionList(item)).ToList(),
                                    CategoryParameter = category.FirstParameter,
                                    Id = target.Id + id + (offset * index)
                                };
                                target.Children.Add(newBranch);
                                this.ScaffoldTypeInternal(elementType, item, newBranch, root);
                            }
                        }
                        // continue;
                    }

                    #endregion array? (must have Category AND CategoryParameter attributes)

                    else
                    {
                        // find or create category branch (even if unfolding is ongoing)
                        if (hasCategory)
                        {
                            var category = GetCategoryName(prop, instance, this._culture);
                            var categoryDescription = GetCategoryDescription(prop);
                            var tget = target.Children.FirstOrDefault(c => c.Category == category.Name);
                            if (filterAttributes.Length > 0)
                            {
                                exclusionList = exclusionList.Union(filterAttributes.SelectMany(x => x.GetExclusionList(instance))).ToList();
                            }

                            if (tget == null)
                            {
                                tget = new ScaffoldedStructureInternal
                                {
                                    Category = category.Name,
                                    Parent = target,
                                    ExclusionList = exclusionList,
                                    Description = categoryDescription,
                                    CategoryParameter = category.FirstParameter,
                                    Id = target.Id + id + offset
                                };
                                target.Children.Add(tget);
                            }
                            target = tget;
                        }

                        if (isSimpleType)
                        {
                            var newId = target.Id + id;
                            if (unfoldingBranch)
                            {
                                if (porpParent.TryGetCustomAttribute<IdAttribute>(out var parent))
                                {
                                    newId += parent.Id;
                                }
                            }

                            var t = new ScaffoldedEntityInternal
                            {
                                Instance = instance,
                                Property = actualProp,
                                Metadata = prop.GetCustomAttributes<Attribute>(),
                                Id = newId,
                                //Id = ++this.idSeed
                            };
                            target.Entities.Add(t);
                        }
                        else if (instance != null)
                        {
                            var propertyValue = actualProp.GetValue(instance);
                            var unfold = prop.GetCustomAttribute<UnfoldAttribute>() != null || unfoldingBranch;
                            // unfold complex type?
                            this.ScaffoldTypeInternal(propertyType, propertyValue, target, root, prop, unfold);
                        }
                    }
                }

                // at the end of all recursions...
                return root.Publish();
            }

            private static string GetCategoryDescription(MemberInfo member)
            {
                if (member.TryGetCustomAttribute<CategoryDescriptionAttribute>(out var description))
                {
                    return description.Description();
                }
                return string.Empty;
            }

            private static (string Name, string FirstParameter) GetCategoryName(MemberInfo member, object instance, CultureInfo culture)
            {
                if (member.TryGetCustomAttribute<CategoryAttribute>(out var category))
                {
                    var categoryProperties = member.GetCustomAttributes<CategoryParameterAttribute>().Select(p =>
                    {
                        if (instance == null)
                        {
                            return null;
                        }
                        // Does the very type on the property contain a 'PropertyReference'-named property?
                        var subPropInfo = p.GetType().GetProperty(p.PropertyReference);
                        if (subPropInfo != null)
                        {
                            var subValue = member.MemberType == MemberTypes.Field ? ((FieldInfo)member).GetValue(instance) : ((PropertyInfo)member).GetValue(instance);
                            if (subValue != null)
                            {
                                return GetUnderlyingType(subPropInfo.GetValue(subValue), culture);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        // Ok, does then the owning type contain a 'PropertyReference'-named property?
                        return GetUnderlyingType(instance.GetType().GetProperty(p.PropertyReference).GetValue(instance), culture);
                    }).ToArray();

                    return (string.Format(culture, category.Category(), categoryProperties), categoryProperties?.FirstOrDefault()?.ToString());
                }
                return (GetDisplayName(member), null);
            }

            /// <summary>
            /// Overload method for ARRAYS.
            /// </summary>
            private static (string Name, string FirstParameter) GetCategoryName(Type itemtype, string format, object item, CultureInfo culture, params CategoryParameterAttribute[] propertyReferences)
            {
                if (!(propertyReferences?.Length > 0))
                {
                    throw new ArgumentNullException(nameof(propertyReferences));
                }
                var categoryProperties = propertyReferences.Select(p =>
                {
                    if (item == null)
                    {
                        return null;
                    }
                    var subPropInfo = itemtype.GetProperty(p.PropertyReference);
                    var itemValue = subPropInfo.GetValue(item);
                    if (p.ValueStringifierType != null)
                    {
                        try
                        {
                            var converter = Activator.CreateInstance(p.ValueStringifierType);
                            if (!(converter is IValueStringifier stringifier))
                            {
                                throw new ScaffoldingException($"{p.ValueStringifierType} does not implement {typeof(IValueStringifier)}.");
                            }
                            return stringifier.Stringify(itemValue, culture);
                        }
                        catch
                        {
                            throw new ScaffoldingException($"Cannot create an instance of {p.ValueStringifierType}. No public empty constructor found.");
                        }
                    }
                    return GetUnderlyingType(itemValue, culture);
                }).ToArray();

                return (string.Format(culture, format, categoryProperties), categoryProperties?.FirstOrDefault()?.ToString());
            }

            private static string GetDisplayName(MemberInfo prop)
            {
                if (prop.TryGetCustomAttribute<DisplayAttribute>(out var displayAttribute))
                {
                    return displayAttribute.GetName();
                }
                return prop.Name;
            }

            private static MemberInfo[] GetMemberInfos(Type type)
            {
                var props = (type ?? throw new ArgumentNullException(nameof(type)))
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Cast<MemberInfo>();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Cast<MemberInfo>();
                return fields.Union(props).ToArray();
            }

            private static object GetUnderlyingType(object obj, CultureInfo culture)
            {
                if (obj is Enum @enum)
                {
                    var baseType = Enum.GetUnderlyingType(@enum.GetType());
                    return System.Convert.ChangeType(obj, baseType, culture);
                }
                return obj;
            }

            private static bool IsSimpleType(Type type)
            {
                var coreType = Nullable.GetUnderlyingType(type) ?? type;
                return coreType == typeof(string) || coreType.IsValueType || coreType.IsSerializable;
            }

            #endregion
        }

        #endregion
    }

    internal class ScaffoldedEntityInternal
    {
        #region Properties

        public int Id { get; set; }

        public object Instance { get; set; }

        public IEnumerable<Attribute> Metadata { get; set; }

        public PropertyInfo Property { get; set; }

        #endregion
    }

    internal class ScaffoldedStructureInternal
    {
        #region Properties

        public string Category { get; set; }

        public string CategoryParameter { get; set; }

        public List<ScaffoldedStructureInternal> Children { get; set; } = new List<ScaffoldedStructureInternal>();

        public string Description { get; set; }

        public List<ScaffoldedEntityInternal> Entities { get; set; } = new List<ScaffoldedEntityInternal>();

        public List<string> ExclusionList { get; set; }

        public int Id { get; set; }

        public ScaffoldedStructureInternal Parent { get; set; }

        #endregion
    }
}
