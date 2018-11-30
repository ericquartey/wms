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
            list => list.ItemListStatus.Equals(ItemListStatus.Completed);

        private static readonly Expression<Func<DataModels.ItemList, bool>> StatusWaitingFilter =
            list => list.ItemListStatus.Equals(ItemListStatus.Waiting);

        private static readonly Expression<Func<DataModels.ItemList, bool>> TypePickFilter =
            list => list.ItemListType.Equals(ItemListType.Pick);

        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;
        private readonly ItemListRowProvider itemListRowProvider;

        #endregion Fields

        #region Constructors

        public ItemListProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            ItemListRowProvider itemListRowProvider)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
            this.itemListRowProvider = itemListRowProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<Int32> Add(ItemListDetails model)
        {
            throw new NotImplementedException();
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemList> GetAll()
        {
            var itemLists = this.dataContext.ItemLists
               .Include(l => l.ItemListRows)
               .Select(l => new ItemList
               {
                   Id = l.Id,
                   Code = l.Code,
                   Description = l.Description,
                   AreaName = l.Area.Name,
                   Priority = l.Priority,
                   ItemListStatusDescription = l.ItemListStatus.ToString(),
                   ItemListTypeDescription = ((ItemListType)l.ItemListType).ToString(),
                   ItemListRowsCount = l.ItemListRows.Count(),
                   ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                   CreationDate = l.CreationDate
               }).AsNoTracking();

            return itemLists;
        }

        public Int32 GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.ItemLists.Count();
            }
        }

        public ItemListDetails GetById(Int32 id)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var itemListDetails = this.dataContext.ItemLists
               .Include(l => l.ItemListRows)
               .Where(l => l.Id == id)
               .Select(l => new ItemListDetails
               {
                   Id = l.Id,
                   Code = l.Code,
                   Description = l.Description,
                   AreaName = l.Area.Name,
                   Priority = l.Priority,
                   ItemListStatus = (ItemListStatus)l.ItemListStatus,
                   ItemListType = (int)((ItemListType)l.ItemListType),
                   ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                   CreationDate = l.CreationDate,
                   Job = l.Job,
                   CustomerOrderCode = l.CustomerOrderCode,
                   CustomerOrderDescription = l.CustomerOrderDescription,
                   ShipmentUnitAssociated = l.ShipmentUnitAssociated,
                   ShipmentUnitCode = l.ShipmentUnitCode,
                   ShipmentUnitDescription = l.ShipmentUnitDescription,
                   LastModificationDate = l.LastModificationDate,
                   FireExecutionDate = l.FirstExecutionDate,
                   ExecutionEndDate = l.ExecutionEndDate,
               }).Single();

                itemListDetails.ItemListStatusChoices = ((ItemListStatus[])
                    Enum.GetValues(typeof(ItemListStatus)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();
                itemListDetails.ItemListTypeChoices = ((ItemListType[])
                    Enum.GetValues(typeof(ItemListType)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();

                itemListDetails.ItemListRows = this.itemListRowProvider.GetByItemListId(id);

                return itemListDetails;
            }
        }

        public IQueryable<ItemList> GetWithStatusCompleted()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, StatusCompletedFilter);
        }

        public Int32 GetWithStatusCompletedCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.ItemLists.AsNoTracking().Count(StatusCompletedFilter);
            }
        }

        public IQueryable<ItemList> GetWithStatusWaiting()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, StatusWaitingFilter);
        }

        public Int32 GetWithStatusWaitingCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.ItemLists.AsNoTracking().Count(StatusWaitingFilter);
            }
        }

        public IQueryable<ItemList> GetWithTypePick()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, TypePickFilter);
        }

        public Int32 GetWithTypePickCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.ItemLists.AsNoTracking().Count(TypePickFilter);
            }
        }

        public Int32 Save(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.ItemLists.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        private static IQueryable<ItemList> GetAllListsWithAggregations(DatabaseContext context, Expression<Func<DataModels.ItemList, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.ItemLists
             .Include(l => l.ItemListRows)
             .Where(actualWhereFunc)
             .Select(l => new ItemList
             {
                 Id = l.Id,
                 Code = l.Code,
                 Description = l.Description,
                 AreaName = l.Area.Name,
                 Priority = l.Priority,
                 ItemListStatusDescription = l.ItemListStatus.ToString(),
                 ItemListTypeDescription = ((ItemListType)l.ItemListType).ToString(),
                 ItemListRowsCount = l.ItemListRows.Count(),
                 ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                 CreationDate = l.CreationDate,
             }).AsNoTracking();
        }

        #endregion Methods
    }
}
