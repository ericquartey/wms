using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class BaseProvider
    {
        #region Constructors

        protected BaseProvider()
        {
        }

        #endregion Constructors

        #region Methods

        protected static async Task<object[]> GetUniqueValuesAsync<TDataModel>(string propertyName, IQueryable<TDataModel> dbSet)
            where TDataModel : class
        {
            var selectExpression = BuildSelectExpression<TDataModel>(propertyName);

            return await dbSet.Select(selectExpression).Distinct().ToArrayAsync();
        }

        private static Expression<Func<TDataModel, object>> BuildSelectExpression<TDataModel>(string propertyName)
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

        #endregion Methods
    }
}
