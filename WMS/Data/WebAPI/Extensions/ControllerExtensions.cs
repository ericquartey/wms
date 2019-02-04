using System;
using System.Linq.Expressions;
using Ferretto.Common.Utils.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Extensions
{
    public static class ControllerExtensions
    {
        #region Methods

        public static Expression<Func<T, bool>> BuildWhereExpression<T>(this ControllerBase controller, string where)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                return null;
            }

            var lambdaInParameter = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());
            var expression = where.AsIExpression();
            var lambdaBody = expression?.GetLambdaBody<T>(lambdaInParameter);

            return (Expression<Func<T, bool>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        #endregion
    }
}
