using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Data.Helpers;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Visitors;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class IQueryableExtension
    {
        #region Fields

        private static readonly Regex OrderByRegex =
            new Regex(
                $@"(?<{nameof(SortOption.PropertyName)}>[^\s]+)\s+(?<{nameof(SortOption.Direction)}>({nameof(ListSortDirection.Ascending)}|{nameof(ListSortDirection.Descending)}))(,\s*)?",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        private static MethodInfo orderByDescendingMethod;

        private static MethodInfo orderByMethod;

        private static MethodInfo thenByDescendingMethod;

        private static MethodInfo thenByMethod;

        #endregion

        #region Methods

        public static async Task<int> CountAsync<TBusinessModel, TDataModel>(
            this IQueryable<TBusinessModel> entities,
            string whereString,
            Expression<Func<TBusinessModel, bool>> searchExpression)
        {
            var whereCriteriaOperator = CriteriaOperator.Parse(whereString);
            whereCriteriaOperator = RemoveNullPropertiesVisitor.Patch(whereCriteriaOperator);
            var containsOnlyTypePropertiesVisitor = new ContainsOnlyTypePropertiesVisitor<TDataModel>();
            var containsOnlyTypeProperties = containsOnlyTypePropertiesVisitor.ProcessCriteria(whereCriteriaOperator);

            var result = await ApplyTransformAsync(
                             entities,
                             0,
                             int.MaxValue,
                             null,
                             whereCriteriaOperator,
                             searchExpression,
                             containsOnlyTypeProperties);

            return containsOnlyTypeProperties ? await result.CountAsync() : result.Count();
        }

        public static IEnumerable<SortOption> ParseSortOptions(this string orderBy)
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
                    true);

                return new SortOption(propertyName, direction);
            });
        }

        public static async Task<IEnumerable<TBusinessModel>> ToArrayAsync<TBusinessModel, TDataModel>(
            this IQueryable<TBusinessModel> entities,
            int skip,
            int take,
            IEnumerable<SortOption> orderBy,
            string whereString,
            Expression<Func<TBusinessModel, bool>> searchExpression)
        {
            var whereCriteriaOperator = CriteriaOperator.Parse(whereString);
            whereCriteriaOperator = RemoveNullPropertiesVisitor.Patch(whereCriteriaOperator);
            var containsOnlyTypePropertiesVisitor = new ContainsOnlyTypePropertiesVisitor<TDataModel>();
            var containsOnlyTypeProperties = containsOnlyTypePropertiesVisitor.ProcessCriteria(whereCriteriaOperator);

            var result = await ApplyTransformAsync(
                             entities,
                             skip,
                             take,
                             orderBy,
                             whereCriteriaOperator,
                             searchExpression,
                             containsOnlyTypeProperties);

            return containsOnlyTypeProperties ? await result.ToArrayAsync() : result.ToArray();
        }

        private static IQueryable<T> ApplyOrderByClause<T>(IEnumerable<SortOption> sortOptions, IQueryable<T> entities)
        {
            if (sortOptions == null)
            {
                return entities;
            }

            var orderedEntities = entities;

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
                    .GetMethod(nameof(CreateSelectorExpression), BindingFlags.Static | BindingFlags.NonPublic)?
                    .MakeGenericMethod(typeof(T), propertyType)
                    .Invoke(null, new object[] { sortOption.PropertyName });

                MethodInfo methodInstance;
                if (sortOption.Direction == ListSortDirection.Ascending)
                {
                    methodInstance =
                        firstOrdering ? GetOrderByMethod<T>(propertyType) : GetThenByMethod<T>(propertyType);
                }
                else
                {
                    methodInstance = firstOrdering
                                         ? GetOrderByDescendingMethod<T>(propertyType)
                                         : GetThenByDescendingMethod<T>(propertyType);
                }

                firstOrdering = false;
                orderedEntities = methodInstance.Invoke(null, new[] { orderedEntities, expression }) as IQueryable<T>;
            }

            return orderedEntities;
        }

        private static async Task<IQueryable<TModel>> ApplyTransformAsync<TModel>(
            IQueryable<TModel> entities,
            int skip,
            int take,
            IEnumerable<SortOption> orderBy,
            CriteriaOperator whereCriteriaOperator,
            Expression<Func<TModel, bool>> searchExpression,
            bool containsOnlyTypeProperties)
        {
            var filteredItems = entities;

            if (containsOnlyTypeProperties)
            {
                filteredItems = (IQueryable<TModel>)filteredItems
                    .AppendWhere(
                        new CriteriaToExpressionConverter(),
                        StringsTolowerCloningHelper.Process(whereCriteriaOperator));
            }
            else
            {
                filteredItems = (IQueryable<TModel>)(await filteredItems.ToArrayAsync())
                    .AsQueryable()
                    .AppendWhere(
                        new CriteriaToExpressionConverter(),
                        StringsTolowerCloningHelper.Process(whereCriteriaOperator));
            }

            // TODO: if skip or take, then orderby should be defined (throw exception)
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

        #endregion
    }
}
