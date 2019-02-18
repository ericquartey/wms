using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using System.Xml.Linq;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ProviderExtension
    {
        #region Methods

        public static async Task<object[]> GetUniqueValuesAsync<TDataModel, TBusinessModel>(
            this IGetUniqueValuesAsyncProvider provider,
            string propertyName,
            IQueryable<TDataModel> dbSet,
            IQueryable<TBusinessModel> boSet)
            where TDataModel : class
            where TBusinessModel : class
        {
            var dataModelPropertyInfo = typeof(TDataModel).GetProperty(propertyName);

            if (dataModelPropertyInfo != null)
            {
                var selectExpression = DoBuildSelectExpression<TDataModel>(propertyName, dataModelPropertyInfo.PropertyType);
                return await dbSet.Select(selectExpression).Distinct().ToArrayAsync();
            }

            var businessModelPropertyInfo = typeof(TBusinessModel).GetProperty(propertyName);
            if (businessModelPropertyInfo == null)
            {
                throw new InvalidOperationException($"The specified property '{propertyName}' is not found");
            }

            var businessSelectExpression = DoBuildSelectExpression<TBusinessModel>(propertyName, businessModelPropertyInfo.PropertyType);
            return await boSet.Select(businessSelectExpression).Distinct().ToArrayAsync();
        }

        private static string ToStringLambda<TProperty>(TProperty property) => property.ToString();

        private static Expression<Func<TDataModel, string>> DoBuildSelectExpression<TDataModel>(
            string propertyName,
            Type propertyType)
            where TDataModel : class
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            var toStringLambda = typeof(ProviderExtension)
                .GetMethod(
                    nameof(ToStringLambda),
                    BindingFlags.Static | BindingFlags.NonPublic)?
                .MakeGenericMethod(propertyType);

            var lambdaInParameter = Expression.Parameter(typeof(TDataModel), typeof(TDataModel).Name.ToLower());
            var propertyAccessor = Expression.Property(lambdaInParameter, propertyName);
            var lambdaBody = Expression.Call(toStringLambda, propertyAccessor);

            return (Expression<Func<TDataModel, string>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        #endregion
    }
}
