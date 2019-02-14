using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class ProviderExtension
    {
        #region Methods

        public static async Task<object[]> GetUniqueValuesAsync<TDataModel>(
            this IGetUniqueValuesAsyncProvider provider,
            string propertyName,
            IQueryable<TDataModel> dbSet)
            where TDataModel : class
        {
            var selectExpression = DoBuildSelectExpression<TDataModel>(propertyName);

            return await dbSet.Select(selectExpression).Distinct().ToArrayAsync();
        }

        private static Expression<Func<TDataModel, object>> DoBuildSelectExpression<TDataModel>(string propertyName)
            where TDataModel : class
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            var lambdaInParameter = Expression.Parameter(typeof(TDataModel), typeof(TDataModel).Name.ToLower());
            var lambdaBody = Expression.Convert(
                Expression.Property(lambdaInParameter, propertyName), typeof(object));

            return (Expression<Func<TDataModel, object>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        #endregion
    }
}
