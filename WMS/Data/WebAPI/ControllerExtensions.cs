using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI
{
    public static class ControllerExtensions
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex orderByRegex =
            new System.Text.RegularExpressions.Regex(
            $@"(?<{nameof(SortOption.PropertyName)}>[^\s]+)\s+(?<{nameof(SortOption.Direction)}>({nameof(ListSortDirection.Ascending)}|{nameof(ListSortDirection.Descending)}))(,\s*)?",
            System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static MethodInfo OrderByDescendingMethod;

        private static MethodInfo OrderByMethod;

        private static MethodInfo ThenByDescendingMethod;

        private static MethodInfo ThenByMethod;

        #endregion Fields

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
                    .SingleOrDefault(p => p.Name.Equals(sortOption.PropertyName, StringComparison.InvariantCultureIgnoreCase))?
                    .PropertyType;

                if (propertyType == null)
                {
                    throw new InvalidOperationException(
                        $"Property {sortOption.PropertyName} does not exist on entity {typeof(T).Name}");
                }

                var expression = typeof(ControllerExtensions)
                    .GetMethod(nameof(CreateSelectorExpression))
                    .MakeGenericMethod(typeof(T), propertyType)
                    .Invoke(null, new[] { sortOption.PropertyName });

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

            return ((Expression<Func<T, TResult>>)
                Expression.Lambda(propertyAccessor, parameter));
        }

        public static IEnumerable<SortOption> ParseOrderByString(this ControllerBase controller, string orderBy)
        {
            if (orderBy == null)
            {
                return new SortOption[0];
            }

            var matches = orderByRegex.Matches(orderBy);

            return matches.Select(match =>
            {
                var propertyName = match.Groups[nameof(SortOption.PropertyName)].Value;

                var direction = (ListSortDirection)Enum.Parse(
                    typeof(ListSortDirection),
                    match.Groups[nameof(SortOption.Direction)].Value,
                    ignoreCase: true);

                return new SortOption(propertyName, direction);
            }
            );
        }

        private static MethodInfo GetOrderByDescendingMethod<T>(Type propertyType)
        {
            if (OrderByDescendingMethod == null)
            {
                var methodName = nameof(Queryable.OrderByDescending);

                OrderByDescendingMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return OrderByDescendingMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetOrderByMethod<T>(Type propertyType)
        {
            if (OrderByMethod == null)
            {
                var methodName = nameof(Queryable.OrderBy);

                OrderByMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return OrderByMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetThenByDescendingMethod<T>(Type propertyType)
        {
            if (ThenByDescendingMethod == null)
            {
                var methodName = nameof(Queryable.ThenByDescending);

                ThenByDescendingMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return ThenByDescendingMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        private static MethodInfo GetThenByMethod<T>(Type propertyType)
        {
            if (ThenByMethod == null)
            {
                var methodName = nameof(Queryable.ThenBy);

                ThenByMethod = typeof(Queryable)
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Single(m =>
                       m.Name == methodName
                       &&
                       m.GetParameters().Count() == 2);
            }

            return ThenByMethod.MakeGenericMethod(typeof(T), propertyType);
        }

        #endregion Methods
    }

    public static class ExpressionExtensions
    {
        #region Methods

        public static Expression GetLambdaBody<T>(this IExpression expression, ParameterExpression inParameter)
        {
            if (expression is Common.Utils.BinaryExpression binaryExpression)
            {
                if (binaryExpression.OperatorName == nameof(Expression.And))
                {
                    return Expression.And(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
                else if (binaryExpression.OperatorName == nameof(Expression.Or))
                {
                    return Expression.Or(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
                else if (binaryExpression.OperatorName == nameof(Expression.Equal))
                {
                    return Expression.Equal(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
            }
            else if (expression is Common.Utils.ValueExpression valueExpression)
            {
                var propertyInfo = typeof(T).GetProperty(valueExpression.Value);

                if (propertyInfo == null)
                {
                    return Expression.Constant(valueExpression.Value);
                }
                else
                {
                    return Expression.Property(inParameter, valueExpression.Value);
                }
            }

            return null;
        }

        #endregion Methods
    }
}
