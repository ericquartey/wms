using Ferretto.VW.App.Scaffolding.DataAnnotations;
using Ferretto.VW.App.Scaffolding.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Scaffolding.Services
{
    public static class MetadataService
    {
        #region NESTED TYPES

        /// <summary>
        /// Use it and throw it away...
        /// </summary>
        class MetadataServiceExecutor
        {
            private int idSeed = 0;

            private static MemberInfo[] GetMemberInfos(Type type)
            {
                var props = (type ?? throw new ArgumentNullException(nameof(type)))
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Cast<MemberInfo>();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Cast<MemberInfo>();
                return fields.Union(props).ToArray();
            }

            private static string GetDisplayName(MemberInfo prop)
            {
                if (prop.TryGetCustomAttribute<DisplayAttribute>(out var displayAttribute))
                {
                    return displayAttribute.GetName();
                }
                return prop.Name;
            }

            private static string GetCategoryName(MemberInfo member, object instance)
            {
                if (member.TryGetCustomAttribute<CategoryAttribute>(out var category))
                {
                    object[] categoryProperties = member.GetCustomAttributes<CategoryParameterAttribute>().Select(p =>
                    {
                        // Does the very type on the property contain a 'PropertyReference'-named property?
                        var subPropInfo = p.GetType().GetProperty(p.PropertyReference);
                        if (subPropInfo != null)
                        {
                            object subValue = member.MemberType == MemberTypes.Field ? ((FieldInfo)member).GetValue(instance) : ((PropertyInfo)member).GetValue(instance); 
                            return GetUnderlyingType(subPropInfo.GetValue(subValue));
                        }
                        // Ok, does then the owning type contain a 'PropertyReference'-named property?
                        return GetUnderlyingType(instance.GetType().GetProperty(p.PropertyReference).GetValue(instance));
                    }).ToArray();

                    IFormatProvider culture = Thread.CurrentThread.CurrentUICulture;
                    return string.Format(culture, category.Category(), categoryProperties);
                }
                return GetDisplayName(member);
            }

            /// <summary>
            /// Overload method for ARRAYS.
            /// </summary>
            private static string GetCategoryName(Type itemtype, string format, object item, params string[] propertyReferences)
            {
                if (!(propertyReferences?.Length > 0))
                {
                    throw new ArgumentNullException(nameof(propertyReferences));
                }
                object[] categoryProperties = propertyReferences.Select(p =>
                {
                    var subPropInfo = itemtype.GetProperty(p);
                    return GetUnderlyingType(subPropInfo.GetValue(item));

                }).ToArray();

                IFormatProvider culture = Thread.CurrentThread.CurrentUICulture;
                return string.Format(culture, format, categoryProperties);
            }

            private static bool IsSimpleType(Type type)
            {
                var coreType = Nullable.GetUnderlyingType(type) ?? type;
                return coreType == typeof(string) || coreType.IsValueType || coreType.IsSerializable;
            }

            private static object GetUnderlyingType(object obj)
            {
                if (obj is Enum @enum)
                {
                    Type baseType = Enum.GetUnderlyingType(@enum.GetType());
                    return System.Convert.ChangeType(obj, baseType);
                }
                return obj;
            }

            public Models.ScaffoldedStructure ScaffoldTypeInternal(Type type, object instance, ScaffoldedStructureInternal branch, ScaffoldedStructureInternal root = default, bool unfoldingBranch = false)
            {
                root = root ?? branch;
                if (instance != null && instance.GetType() != type)
                {
                    throw new ArgumentException($"Type mismatch: '{ instance.GetType() }' does not match '{ type }'.");
                }
                var modelType = type.GetCustomAttribute<MetadataTypeAttribute>()?.MetadataClassType ?? type;
                Dictionary<string, List<Type>> dict = new Dictionary<string, List<Type>>();

                foreach (var prop in GetMemberInfos(modelType))
                {
                    PropertyInfo actualProp = type.GetProperty(prop.Name);
                    Type propertyType = prop.MemberType == MemberTypes.Field ? ((FieldInfo)prop).FieldType : ((PropertyInfo)prop).PropertyType;
                    ScaffoldedStructureInternal target = branch;

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
                    bool hasCategory = prop.TryGetCustomAttribute<CategoryAttribute>(out var categoryAttr);
                    var categoryParameters = prop.GetCustomAttributes<CategoryParameterAttribute>();
                    bool hasCategoryParameters = categoryParameters.Any();
                    bool isSimpleType = IsSimpleType(propertyType);

                    #region  array? (must have Category AND CategoryParameter attributes)
                    // flattening
                    if (!isSimpleType && (typeof(System.Collections.IEnumerable)).IsAssignableFrom(propertyType))
                    {
                        if (!hasCategory)
                        {
                            throw new ScaffoldingException($"Cannot find a {nameof(CategoryAttribute)} on this enumerable property.");
                        }
                        if (!hasCategoryParameters)
                        {
                            throw new ScaffoldingException($"Cannot find at least one {nameof(CategoryParameterAttribute)} on this enumerable property.");
                        }
                        Type elementType = propertyType.GetElementType() ?? propertyType.GetGenericArguments().Single();

                        if (instance == null)
                        {

                            // cannot retrieve flattening properties from a null instance, skipping...
                            continue;
                        }

                        if (actualProp.GetValue(instance) is System.Collections.IEnumerable collection)
                        {
                            string format = categoryAttr.Category();
                            string[] propertyReferences = categoryParameters.Select(c => c.PropertyReference).ToArray();
                            foreach (var item in collection)
                            {
                                string categoryName = GetCategoryName(elementType, format, item, propertyReferences);
                                var newBranch = target.Children.FirstOrDefault(b => b.Category == categoryName);
                                if (newBranch != null)
                                {
                                    throw new ScaffoldingException($"A category with the name {categoryName} already exists.");
                                }
                                newBranch = new ScaffoldedStructureInternal
                                {
                                    Category = categoryName,
                                    Parent = target
                                };
                                target.Children.Add(newBranch);
                                this.ScaffoldTypeInternal(elementType, item, newBranch, root);
                            }
                        }
                        // continue;
                    }
                    #endregion

                    else
                    {
                        // find or create category branch
                        if (hasCategory)
                        {
                            string categoryName = GetCategoryName(prop, instance);
                            var tget = target.Children.FirstOrDefault(c => c.Category == categoryName);
                            if (tget == null)
                            {
                                tget = new ScaffoldedStructureInternal
                                {
                                    Category = categoryName,
                                    Parent = target
                                };
                                target.Children.Add(tget);
                            }
                            target = tget;
                        }

                        if (isSimpleType)
                        {
                            target.Entities.Add(new ScaffoldedEntityInternal
                            {
                                Instance = instance,
                                Property = actualProp,
                                Metadata = prop.GetCustomAttributes<Attribute>(),
                                Id = ++this.idSeed
                            });
                        }
                        else
                        {
                            object propertyValue = actualProp.GetValue(instance);
                            bool unfold = prop.GetCustomAttribute<UnfoldAttribute>() != null || unfoldingBranch;
                            // unfold complex type?
                            this.ScaffoldTypeInternal(propertyType, propertyValue, target, root, unfold);
                        }
                    }
                }

                // at the end of all recursions...
                return root.Publish();
            }

        }

        #endregion

        #region PRIVATE


        private static Models.ScaffoldedEntity Publish(this ScaffoldedEntityInternal entity)
            => new Models.ScaffoldedEntity(entity.Property, entity.Instance, entity.Metadata, entity.Id);

        private static Models.ScaffoldedStructure Publish(this ScaffoldedStructureInternal tree)
        {
            return new Models.ScaffoldedStructure(tree.Category, tree.Entities.Select(e => e.Publish()), tree.Children.Select(c => c.Publish()));
        }

        #endregion

        #region PUBLIC

        public static bool TryGetCustomAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute
        {
            attribute = (member ?? throw new ArgumentNullException(nameof(member))).GetCustomAttribute<T>();
            return attribute != null;
        }

        public static Models.ScaffoldedStructure Scaffold(this object instance) => new MetadataServiceExecutor().ScaffoldTypeInternal(instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)), instance, new ScaffoldedStructureInternal());

        #endregion
    }

    internal class ScaffoldedStructureInternal
    {
        public string Category { get; set; }

        public ScaffoldedStructureInternal Parent { get; set; }

        public List<ScaffoldedEntityInternal> Entities { get; set; } = new List<ScaffoldedEntityInternal>();

        public List<ScaffoldedStructureInternal> Children { get; set; } = new List<ScaffoldedStructureInternal>();
    }

    internal class ScaffoldedEntityInternal
    {
        public IEnumerable<Attribute> Metadata { get; set; }
        public PropertyInfo Property { get; set; }
        public object Instance { get; set; }
        public int Id { get; set; }
    }
}
