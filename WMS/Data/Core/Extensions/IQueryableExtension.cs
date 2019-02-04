using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class IQueryableExtension
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

        public static IQueryable<T> ApplyTransform<T>(
            this IQueryable<T> entities,
            Expression<Func<T, bool>> whereExpression,
            Expression<Func<T, bool>> searchExpression)
        {
            return ApplyTransform(
                entities,
                skip: 0,
                take: int.MaxValue,
                orderBy: null,
                whereExpression: whereExpression,
                searchExpression: searchExpression);
        }

        public static IQueryable<T> ApplyTransform<T>(
            this IQueryable<T> entities,
            int skip,
            int take,
            string orderBy,
            Expression<Func<T, bool>> whereExpression,
            Expression<Func<T, bool>> searchExpression)
        {
            // TODO: if skip or take, then orderby should be defined (throw exception)
            var filteredItems = entities;
            if (whereExpression != null)
            {
                filteredItems = filteredItems.Where(whereExpression);
            }

            if (searchExpression != null)
            {
                filteredItems = filteredItems.Where(searchExpression);
            }

            filteredItems = ApplyOrderByClause(orderBy, filteredItems);

            var skipValue = skip < 0 ? 0 : skip;
            if (skipValue > 0)
            {
                filteredItems = filteredItems.Skip(skipValue);
            }

            var takeValue = take < 0 ? int.MaxValue : take;
            if (takeValue != int.MaxValue)
            {
                filteredItems = filteredItems.Take(takeValue);
            }

            return filteredItems;
        }

        private static IQueryable<T> ApplyOrderByClause<T>(string orderBy, IQueryable<T> entities)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return entities;
            }

            var orderedEntities = entities;

            var sortOptions = ParseOrderByString(orderBy);

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

                var expression = typeof(IQueryableExtension)
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

        private static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var propertyAccessor = Expression.PropertyOrField(parameterExpression, propertyName);
            return (Expression<Func<T, TResult>>)Expression.Lambda(propertyAccessor, parameterExpression);
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

        private static IEnumerable<SortOption> ParseOrderByString(string orderBy)
        {
            if (orderBy == null)
            {
                return Array.Empty<SortOption>();
            }

            var matches = OrderByRegex.Matches(orderBy);

            return matches.Cast<Match>().Select(match =>
            {
                var propertyName = match.Groups[nameof(SortOption.PropertyName)].Value;

                var direction = (ListSortDirection)Enum.Parse(
                    typeof(ListSortDirection),
                    match.Groups[nameof(SortOption.Direction)].Value,
                    true);

                return new SortOption(propertyName, direction);
            });
        }

        #endregion
    }
}
