using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    public static class ControllerExtensions
    {
        #region Methods

        public static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(this ControllerBase controller, string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, TResult>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, propertyName),
                                                                    parameterExpression);
        }

        #endregion Methods
    }
}
