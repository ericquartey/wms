using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerContext : DbContext
    {
        #region Methods

        public void AddOrUpdate<T, TKey>(T entity, Func<T, TKey> idExpression)
            where T : class
        {
            if (idExpression is null)
            {
                throw new ArgumentNullException(nameof(idExpression));
            }

            if (entity != null)
            {
                var existingEntity = this.Set<T>().Find(idExpression(entity));
                if (existingEntity != null)
                {
                    this.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    this.Set<T>().Add(entity);
                }
            }
        }

        public void Delete<TSource, TResult>(IEnumerable<TSource> entity, Func<TSource, TResult> selector)
            where TSource : class
        {
            if (entity != null)
            {
                var result = this.Set<TSource>().Select(selector).Except(entity.Select(selector)).ToList();
                result.ForEach(f => this.Set<TSource>().Remove(this.Set<TSource>().Find(f)));
            }
        }

        #endregion
    }
}
