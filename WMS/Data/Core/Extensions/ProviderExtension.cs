using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Models;
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

        public static async Task<IOperationResult<TBusinessModel>> UpdateAsync<TDataModel, TBusinessModel, TKey>(
            this IUpdateAsyncProvider<TBusinessModel, TKey> provider,
            TBusinessModel model,
            DbSet<TDataModel> dbSet,
            DatabaseContext dataContext)
            where TBusinessModel : class, IModel<TKey>, IPolicyDescriptor<Policy>
            where TDataModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (provider is IReadSingleAsyncProvider<TBusinessModel, TKey> readProvider)
            {
                var existingBusinessModel = await readProvider.GetByIdAsync(model.Id);
                if (existingBusinessModel == null)
                {
                    return new NotFoundOperationResult<TBusinessModel>();
                }

                if (!existingBusinessModel.CanUpdate())
                {
                    return new UnprocessableEntityOperationResult<TBusinessModel>
                    {
                        Description = existingBusinessModel.GetCanUpdateReason(),
                    };
                }
            }

            var existingDataModel = dbSet.Find(model.Id);
            dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await dataContext.SaveChangesAsync();

            return new SuccessOperationResult<TBusinessModel>(model);
        }

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

        private static string ToStringLambda<TProperty>(TProperty property) => property?.ToString();

        #endregion
    }
}
