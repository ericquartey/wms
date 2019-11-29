using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Ferretto.VW.App.Controls.Controls.Keyboards.Demo
{
    public static class ReflectionUtils
    {
        /* Some cache data structures */

        #region Fields

        private static readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        private static readonly Dictionary<CacheKey, List<MemberInfo>> collectionMembersCache = new Dictionary<CacheKey, List<MemberInfo>>();

        private static readonly Dictionary<CacheKey, Dictionary<string, MemberInfo>> membersCache = new Dictionary<CacheKey, Dictionary<string, MemberInfo>>();

        private static readonly Dictionary<Type, Dictionary<BindingFlags, Dictionary<MemberTypes, Dictionary<string, MemberInfo>>>> superCache = new Dictionary<Type, Dictionary<BindingFlags, Dictionary<MemberTypes, Dictionary<string, MemberInfo>>>>();

        #endregion

        #region Methods

        /// <summary>
        /// Assigns source properties to properties of target object retrieved using the map function.
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="bindingFlags">Binding flags used to retrive source properties</param>
        /// <param name="mapFunction">The map function used to retrive target properties</param>
        public static void Assign(object source, object target, BindingFlags bindingFlags, Func<PropertyInfo, object, PropertyInfo> mapFunction)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (mapFunction == null)
            {
                throw new ArgumentNullException("mapFunction");
            }

            PropertyInfo[] properties = source.GetType().GetProperties(bindingFlags);

            foreach (PropertyInfo sourceProp in properties.Where((info) => info.CanWrite))
            {
                PropertyInfo targetProp = mapFunction.Invoke(sourceProp, target);
                if (targetProp != null)
                {
                    targetProp.SetValue(target, Convert.ChangeType(sourceProp.GetValue(source, null), targetProp.PropertyType, null), null);
                }
            }
        }

        /// <summary>
        /// Assigns source properties to properties of target object which have same name.
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="bindingFlags">Binding flags used to retrive source properties</param>
        public static void Assign(object source, object target, BindingFlags bindingFlags)
        {
            Assign(source, target, bindingFlags, (pSource, targetObject) =>
            {
                return targetObject.GetType().GetProperty(pSource.Name, bindingFlags);
            });
        }

        /// <summary>
        /// Assigns public source properties to public properties of target object which have same name.
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="bindingFlags">Binding flags used to retrive source properties</param>
        public static void Assign(object source, object target)
        {
            Assign(source, target, BindingFlags.Instance | BindingFlags.Public);
        }

        public static ConstructorInfo[] GetConstructors(Type type)
        {
            return GetMembers(type, MemberTypes.Constructor, BindingFlags.Instance | BindingFlags.Public).Cast<ConstructorInfo>().ToArray();
        }

        public static object GetCustomAttribute(Type targetType, Type attrType, bool inherit)
        {
            object[] attrs = targetType.GetCustomAttributes(attrType, inherit);
            if (attrs.Length > 0)
            {
                return attrs[0];
            }

            return null;
        }

        public static Attribute GetCustomAttribute(MemberInfo memeber, Type attrType, bool inherit)
        {
            return Attribute.GetCustomAttribute(memeber, attrType, inherit);
        }

        public static object GetCustomAttribute(Type targetType, Type attrType)
        {
            return ReflectionUtils.GetCustomAttribute(targetType, attrType, true);
        }

        public static Attribute GetCustomAttribute(MemberInfo member, Type attrType)
        {
            return ReflectionUtils.GetCustomAttribute(member, attrType, true);
        }

        public static T GetCustomAttribute<T>(Type targetType, bool inherit)
        {
            return (T)ReflectionUtils.GetCustomAttribute(targetType, typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(MemberInfo member, bool inherit)
            where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(Type targetType)
        {
            return ReflectionUtils.GetCustomAttribute<T>(targetType, true);
        }

        public static T GetCustomAttribute<T>(MemberInfo member)
            where T : Attribute
        {
            return ReflectionUtils.GetCustomAttribute<T>(member, true);
        }

        public static FieldInfo GetField(Type type, string fieldName, BindingFlags bindingFlags)
        {
            return type.GetField(fieldName, bindingFlags);
            //return GetMember(type, fieldName, MemberTypes.Field, bindingFlags) as FieldInfo;
        }

        public static FieldInfo[] GetHierarchyFields(Type type)
        {
            return GetMembers(type, MemberTypes.Field, BindingFlags.Instance | BindingFlags.FlattenHierarchy).Cast<FieldInfo>().ToArray();
        }

        public static MethodInfo[] GetHierarchyMethods(Type type)
        {
            return GetMembers(type, MemberTypes.Method, BindingFlags.Instance | BindingFlags.FlattenHierarchy).Cast<MethodInfo>().ToArray();
        }

        public static PropertyInfo[] GetHierarchyProperties(Type type)
        {
            return GetMembers(type, MemberTypes.Property, BindingFlags.Instance | BindingFlags.FlattenHierarchy).Cast<PropertyInfo>().ToArray();
        }

        public static MemberInfo GetMember(Type type, string memberName, MemberTypes memberTypes, BindingFlags bindingFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException("memberName");
            }

            //if (memberTypes.HasAnyFlags(MemberTypes.Custom | MemberTypes.NestedType))
            //{
            //    throw new ArgumentOutOfRangeException("memberTypes");
            //}

            //lock (membersCache)
            //{
            //    while (type != null)
            //    {
            //        // Produce the key for our cache data structure
            //        CacheKey key = new CacheKey()
            //        {
            //            Type = type,
            //            MemberTypes = memberTypes,
            //            BindingFlags = bindingFlags
            //        };

            //        if (membersCache.ContainsKey(key) && membersCache[key].ContainsKey(memberName))
            //            return membersCache[key][memberName];

            //        if (!membersCache.ContainsKey(key))
            //            membersCache.Add(key, new Dictionary<string, MemberInfo>());

            //        if (!membersCache[key].ContainsKey(memberName))
            //        {
            //            MemberInfo[] members = type.GetMember(memberName, bindingFlags);

            //            // If there is no members with the provided name
            //            // we add a null placeholder to avoid to repeat
            //            // this task the next time.
            //            if (members.Length == 0)
            //            {
            //                membersCache[key].Add(memberName, null);
            //                type = type.BaseType;
            //            }

            //            else
            //            {
            //                foreach (MemberInfo m in members)
            //                {
            //                    CacheKey _key = new CacheKey()
            //                    {
            //                        Type = type,
            //                        MemberTypes = m.MemberType,
            //                        BindingFlags = bindingFlags
            //                    };

            //                    if (!membersCache.ContainsKey(key))
            //                        membersCache.Add(_key, new Dictionary<string, MemberInfo>());

            //                    membersCache[_key].Add(memberName, m);
            //                }

            //                return membersCache[key][memberName];
            //            }
            //        }
            //    }
            //    // The member we look for does not exists
            //    return null;
            //}

            //lock (superCache)
            //{
            //    while (type != null)
            //    {
            //        if (!superCache.ContainsKey(type))
            //            superCache.Add(type, new Dictionary<BindingFlags, Dictionary<MemberTypes, Dictionary<string, MemberInfo>>>());

            //        if (!superCache[type].ContainsKey(bindingFlags))
            //            superCache[type].Add(bindingFlags, new Dictionary<MemberTypes, Dictionary<string, MemberInfo>>());

            //        if (!superCache[type][bindingFlags].ContainsKey(memberTypes))
            //            superCache[type][bindingFlags].Add(memberTypes, new Dictionary<string, MemberInfo>());

            //        if (!superCache[type][bindingFlags][memberTypes].ContainsKey(memberName))
            //        {
            //            MemberInfo[] members = type.GetMember(memberName, bindingFlags);

            //            if (members.Length == 0)
            //            {
            //                superCache[type][bindingFlags][memberTypes].Add(memberName, null);
            //                type = type.BaseType;
            //            }
            //            else
            //            {
            //                foreach (MemberInfo m in members)
            //                {
            //                    if (!superCache[type][bindingFlags].ContainsKey(m.MemberType))
            //                        superCache[type][bindingFlags].Add(memberTypes, new Dictionary<string, MemberInfo>());

            //                    superCache[type][bindingFlags][memberTypes].Add(memberName, m);
            //                }

            //                return superCache[type][bindingFlags][memberTypes][memberName];
            //            }
            //        }
            //        else
            //            return superCache[type][bindingFlags][memberTypes][memberName];
            //    }
            //    return null;
            //}

            MemberInfo[] membersInfo = type.GetMember(memberName, bindingFlags);
            return membersInfo.Length > 0 ? membersInfo[0] : null;
        }

        public static MemberInfo[] GetMembers(Type type, MemberTypes memberType, BindingFlags bindingFlags)
        {
            CacheKey key = new CacheKey()
            {
                Type = type,
                MemberTypes = memberType,
                BindingFlags = bindingFlags
            };

            lock (collectionMembersCache)
            {
                if (collectionMembersCache.ContainsKey(key))
                {
                    return collectionMembersCache[key].ToArray();
                }

                MemberInfo[] members = null;
                switch (memberType)
                {
                    case MemberTypes.Constructor:
                        members = type.GetConstructors(key.BindingFlags);
                        break;

                    case MemberTypes.Custom:
                        break;

                    case MemberTypes.Event:
                        members = type.GetEvents(key.BindingFlags);
                        break;

                    case MemberTypes.Field:
                        members = type.GetFields(key.BindingFlags);
                        break;

                    case MemberTypes.Method:
                        members = type.GetMethods(key.BindingFlags);
                        break;

                    case MemberTypes.NestedType:
                        members = type.GetNestedTypes(key.BindingFlags);
                        break;

                    case MemberTypes.Property:
                        members = type.GetProperties(key.BindingFlags);
                        break;

                    default:
                        members = type.GetMembers(key.BindingFlags);
                        break;
                }
                if (members != null)
                {
                    collectionMembersCache[key] = new List<MemberInfo>(members);
                }

                return members;
            }
        }

        public static MethodInfo GetMethod(Type type, string methodName, BindingFlags bindingFlags)
        {
            return type.GetMethod(methodName, bindingFlags);
            //return GetMember(type, methodName, MemberTypes.Method, bindingFlags) as MethodInfo;
        }

        public static FieldInfo[] GetPrivateFields(Type type)
        {
            return GetMembers(type, MemberTypes.Field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).Cast<FieldInfo>().ToArray();
        }

        public static MethodInfo[] GetPrivateMethods(Type type)
        {
            return GetMembers(type, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).Cast<MethodInfo>().ToArray();
        }

        public static PropertyInfo[] GetPrivateProperties(Type type)
        {
            return GetMembers(type, MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).Cast<PropertyInfo>().ToArray();
        }

        public static PropertyInfo GetProperty(Type type, string propertyName, BindingFlags bindingFlags)
        {
            return type.GetProperty(propertyName, bindingFlags);
            //return GetMember(type, propertyName, MemberTypes.Property, bindingFlags) as PropertyInfo;
        }

        public static FieldInfo[] GetPublicFields(Type type)
        {
            return GetMembers(type, MemberTypes.Field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Cast<FieldInfo>().ToArray();
        }

        public static MethodInfo[] GetPublicMethods(Type type)
        {
            return GetMembers(type, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Cast<MethodInfo>().ToArray();
        }

        /// <summary>
        /// Gets all instance properties static and public.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPublicProperties(Type type)
        {
            return GetMembers(type, MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Cast<PropertyInfo>().ToArray();
        }

        /// <summary>
        /// Load an assemby by its file path.
        /// </summary>
        /// <param name="assemblyFile">The assembly file path</param>
        /// <returns>Assembly object</returns>
        public static Assembly LoadAssembly(string assemblyFile)
        {
            Assembly assembly = null;
            lock (assemblies)
            {
                if (assemblies.ContainsKey(assemblyFile))
                {
                    assembly = assemblies[assemblyFile];
                }
                else
                {
                    assembly = Assembly.LoadFrom(assemblyFile);
                    assemblies[assemblyFile] = assembly;
                }
            }
            return assembly;
        }

        #endregion

        #region Structs

        private struct CacheKey
        {
            #region Fields

            public BindingFlags BindingFlags;

            public MemberTypes MemberTypes;

            public Type Type;

            #endregion
        }

        #endregion

        /* ************************ */
        //public static Assembly LoadAssembly(string assemblyFile, out AppDomain appDomain)
        //{
        //    appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

        //    int ep = appDomain.ExecuteAssembly(assemblyFile);

        //    Assembly assembly = null;
        //    lock (assemblies)
        //    {
        //        if (assemblies.ContainsKey(assemblyFile))
        //            assembly = assemblies[assemblyFile];
        //        else
        //        {
        //            assembly = Assembly.LoadFrom(assemblyFile);
        //            assemblies[assemblyFile] = assembly;
        //        }
        //    }
        //    return assembly;
        //}
    }
}
