using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ferretto.Common.Utils.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI
{
    public static class ControllerExtensions
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex OrderByRegex =
            new System.Text.RegularExpressions.Regex(
            $@"(?<{nameof(SortOption.PropertyName)}>[^\s]+)\s+(?<{nameof(SortOption.Direction)}>({nameof(ListSortDirection.Ascending)}|{nameof(ListSortDirection.Descending)}))(,\s*)?",
            System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static MethodInfo orderByDescendingMethod;

        private static MethodInfo orderByMethod;

        private static MethodInfo thenByDescendingMethod;

        private static MethodInfo thenByMethod;

        #endregion

        #region Methods

        public static IQueryable<T> ApplyOrderByClause<T>(this ControllerBase controller, string orderBy, IQueryable<T> entities)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return entities;
            }

            var orderedEntities = entities;

            var sortOptions = ParseOrderByString(controller, orderBy);

            var firstOrdering = true;
            foreach (var sortOption in sortOptions)
            {
                var propertyType = typeof(T)
                    .GetProperties()
                    .SingleOrDefault(p => p.Name.Equals(sortOption.PropertyName, StringComparison.Ordinal))?
                    .PropertyType;

                if (propertyType == null)
                {
                    throw new InvalidOperationException(
                        $"Property {sortOption.PropertyName} does not exist on entity {typeof(T).Name}");
                }

                var expression = typeof(ControllerExtensions)
                    .GetMethod(nameof(CreateSelectorExpression))
                    .MakeGenericMethod(typeof(T), propertyType)
                    .Invoke(null, new object[] { sortOption.PropertyName });

                MethodInfo methodInstance;
                if (sortOption.Direction == ListSortDirection.Ascending)
                {
                    methodInstance = firstOrdering ?
                        GetOrderByMethod<T>(propertyType) :
                        GetThenByMethod<T>(propertyType);
                }
                else
                {
                    methodInstance = firstOrdering ?
                       GetOrderByDescendingMethod<T>(propertyType) :
                       GetThenByDescendingMethod<T>(propertyType);
                }

                firstOrdering = false;
                orderedEntities = methodInstance.Invoke(null, new[] { orderedEntities, expression }) as IQueryable<T>;
            }

            return orderedEntities;
        }

        public static bool ContainsComputedFields<TData>(this ControllerBase controller, IEnumerable<SortOption> orderByFields)
        {
            return orderByFields.Any(f => typeof(TData).GetProperty(f.PropertyName) == null);
        }

        public static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");

            var propertyAccessor = Expression.Property(parameter, propertyName);

            return (Expression<Func<T, TResult>>)Expression.Lambda(propertyAccessor, parameter);
        }

        public static IEnumerable<SortOption> ParseOrderByString(this ControllerBase controller, string orderBy)
        {
            if (orderBy == null)
            {
                return Array.Empty<SortOption>();
            }

            var matches = OrderByRegex.Matches(orderBy);

            return matches.Select(match =>
            {
                var propertyName = match.Groups[nameof(SortOption.PropertyName)].Value;

                var direction = (ListSortDirection)Enum.Parse(
                    typeof(ListSortDirection),
                    match.Groups[nameof(SortOption.Direction)].Value,
                    ignoreCase: true);

                return new SortOption(propertyName, direction);
            });
        }

        private static MethodInfo GetOrderByDescendingMethod<T>(Type propertyType)
        {
            if (orderByDescendingMethod == null)
            {
                const string methodName = nameof(Queryable.OrderByDescending);

                orderByDescendingMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return orderByDescendingMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetOrderByMethod<T>(Type propertyType)
        {
            if (orderByMethod == null)
            {
                const string methodName = nameof(Queryable.OrderBy);

                orderByMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return orderByMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetThenByDescendingMethod<T>(Type propertyType)
        {
            if (thenByDescendingMethod == null)
            {
                const string methodName = nameof(Queryable.ThenByDescending);

                thenByDescendingMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return thenByDescendingMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetThenByMethod<T>(Type propertyType)
        {
            if (thenByMethod == null)
            {
                const string methodName = nameof(Queryable.ThenBy);

                thenByMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return thenByMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        #endregion
    }
}
