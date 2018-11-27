using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListProvider : IItemListProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.ItemList, bool>> StatusCompletedFilter =
            list => list.ItemListStatusId == 3;

        private static readonly Expression<Func<DataModels.ItemList, bool>> StatusWaitingFilter =
            list => list.ItemListStatusId == 1;

        private static readonly Expression<Func<DataModels.ItemList, bool>> TypePickFilter =
            list => list.ItemListTypeId == 1;

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public ItemListProvider(
            DatabaseContext dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<Int32> Add(ItemListRow model)
        {
            throw new NotImplementedException();
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemList> GetAll()
        {
            return this.dataContext.ItemLists
               .Include(l => l.ItemListStatus)
               .Include(l => l.ItemListType)
               .Include(l => l.ItemListRows)
               .Select(l => new ItemList
               {
                   Id = l.Id,
                   Code = l.Code,
                   Description = l.Description,
                   AreaName = l.Area.Name,
                   Priority = l.Priority,
                   ItemListStatusDescription = l.ItemListStatus.Description,
                   ItemListTypeDescription = l.ItemListType.Description,
                   ItemListRowsCount = l.ItemListRows.Count(),
                   ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                   CreationDate = l.CreationDate,
                   //TODO: n oggetti
               }).AsNoTracking();
        }

        public Int32 GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ItemLists.Count();
            }
        }

        public ItemListRow GetById(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemList> GetWithStatusCompleted()
        {
            return GetAllListsWithAggregations(this.dataContext, StatusCompletedFilter);
        }

        public Int32 GetWithStatusCompletedCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ItemLists.AsNoTracking().Count(StatusCompletedFilter);
            }
        }

        public IQueryable<ItemList> GetWithStatusWaiting()
        {
            return GetAllListsWithAggregations(this.dataContext, StatusWaitingFilter);
        }

        public Int32 GetWithStatusWaitingCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ItemLists.AsNoTracking().Count(StatusWaitingFilter);
            }
        }

        public IQueryable<ItemList> GetWithTypePick()
        {
            return GetAllListsWithAggregations(this.dataContext, TypePickFilter);
        }

        public Int32 GetWithTypePickCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ItemLists.AsNoTracking().Count(TypePickFilter);
            }
        }

        public Int32 Save(ItemListRow model)
        {
            throw new NotImplementedException();
        }

        private static IQueryable<ItemList> GetAllListsWithAggregations(DatabaseContext context, Expression<Func<DataModels.ItemList, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.ItemLists
             .Include(l => l.ItemListStatus)
             .Include(l => l.ItemListType)
             .Include(l => l.ItemListRows)
             .Where(actualWhereFunc)
             .Select(l => new ItemList
             {
                 Id = l.Id,
                 Code = l.Code,
                 Description = l.Description,
                 AreaName = l.Area.Name,
                 Priority = l.Priority,
                 ItemListStatusDescription = l.ItemListStatus.Description,
                 ItemListTypeDescription = l.ItemListType.Description,
                 ItemListRowsCount = l.ItemListRows.Count(),
                 ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                 CreationDate = l.CreationDate,
             }).AsNoTracking();
        }

        #endregion Methods
    }
}
