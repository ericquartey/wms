using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListProvider : IItemListProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.ItemList, bool>> TypeInventoryFilter =
            list => (char)list.ItemListType == (char)(ItemListType.Inventory);

        private static readonly Expression<Func<DataModels.ItemList, bool>> TypePickFilter =
            list => (char)list.ItemListType == (char)(ItemListType.Pick);

        private static readonly Expression<Func<DataModels.ItemList, bool>> TypePutFilter =
            list => (char)list.ItemListType == (char)(ItemListType.Put);

        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;
        private readonly ItemListRowProvider itemListRowProvider;
        private readonly WMS.Scheduler.WebAPI.Contracts.IItemListsService itemListService;

        #endregion Fields

        #region Constructors

        public ItemListProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            ItemListRowProvider itemListRowProvider,
            WMS.Scheduler.WebAPI.Contracts.IItemListsService itemListService)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
            this.itemListRowProvider = itemListRowProvider;
            this.itemListService = itemListService;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> Add(ItemListDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> ExecuteImmediately(int listId, int areaId, int bayId)
        {
            try
            {
                await this.itemListService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListExecutionRequest { ListId = listId, AreaId = areaId, BayId = bayId });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public IQueryable<ItemList> GetAll()
        {
            var itemLists = this.dataContext.Current.ItemLists
               .Include(l => l.ItemListRows)
               .Select(l => new ItemList
               {
                   Id = l.Id,
                   Code = l.Code,
                   Description = l.Description,
                   Priority = l.Priority,
                   ItemListStatus = (ItemListStatus)l.Status,
                   ItemListType = (ItemListType)l.ItemListType,
                   ItemListRowsCount = l.ItemListRows.Count(),
                   ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                   CreationDate = l.CreationDate
               }).AsNoTracking();

            return itemLists;
        }

        public int GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.ItemLists.Count();
            }
        }

        public async Task<ItemListDetails> GetById(int id)
        {
            var dataContext = this.dataContext.Current;

            var itemListDetails = await this.dataContext.Current.ItemLists
               .Include(l => l.ItemListRows)
               .Where(l => l.Id == id)
               .Select(l => new ItemListDetails
               {
                   Id = l.Id,
                   Code = l.Code,
                   Description = l.Description,
                   Priority = l.Priority,
                   ItemListStatus = (ItemListStatus)l.Status,
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
                   FirstExecutionDate = l.FirstExecutionDate,
                   ExecutionEndDate = l.ExecutionEndDate,
               }).SingleAsync();

            itemListDetails.ItemListStatusChoices = ((ItemListStatus[])
                Enum.GetValues(typeof(ItemListStatus)))
                .Select(i => new Enumeration((int)i, i.ToString())).ToList();
            itemListDetails.ItemListTypeChoices = ((ItemListType[])
                Enum.GetValues(typeof(ItemListType)))
                .Select(i => new Enumeration((int)i, i.ToString())).ToList();

            itemListDetails.ItemListRows = this.itemListRowProvider.GetByItemListId(id);

            return itemListDetails;
        }

        public IQueryable<ItemList> GetWithStatusCompleted(ItemListType? type)
        {
            var filter = BuildFilter(type, ItemListStatus.Completed);
            return GetAllListsWithAggregations(this.dataContext.Current, filter);
        }

        public int GetWithStatusCompletedCount(ItemListType? type)
        {
            var filter = BuildFilter(type, ItemListStatus.Completed);
            return this.dataContext.Current.ItemLists.AsNoTracking().Count(filter);
        }

        public IQueryable<ItemList> GetWithStatusWaiting(ItemListType? type)
        {
            var filter = BuildFilter(type, ItemListStatus.Waiting);
            return GetAllListsWithAggregations(this.dataContext.Current, filter);
        }

        public int GetWithStatusWaitingCount(ItemListType? type)
        {
            var filter = BuildFilter(type, ItemListStatus.Waiting);
            return this.dataContext.Current.ItemLists.AsNoTracking().Count(filter);
        }

        public IQueryable<ItemList> GetWithTypeInventory()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, TypeInventoryFilter);
        }

        public int GetWithTypeInventoryCount()
        {
            return this.dataContext.Current.ItemLists.AsNoTracking().Count(TypeInventoryFilter);
        }

        public IQueryable<ItemList> GetWithTypePick()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, TypePickFilter);
        }

        public int GetWithTypePickCount()
        {
            return this.dataContext.Current.ItemLists.AsNoTracking().Count(TypePickFilter);
        }

        public IQueryable<ItemList> GetWithTypePut()
        {
            return GetAllListsWithAggregations(this.dataContext.Current, TypePutFilter);
        }

        public int GetWithTypePutCount()
        {
            return this.dataContext.Current.ItemLists.AsNoTracking().Count(TypePutFilter);
        }

        public int Save(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var dataContext = this.dataContext.Current;

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Current.ItemLists.Find(model.Id);

                this.dataContext.Current.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
            }
        }

        public async Task<OperationResult> ScheduleForExecution(int listId, int areaId)
        {
            try
            {
                await this.itemListService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListExecutionRequest { ListId = listId, AreaId = areaId });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        private static Expression<Func<DataModels.ItemList, bool>> BuildFilter(ItemListType? type, ItemListStatus status)
        {
            var listType = type.HasValue ? (DataModels.ItemListType)type.Value : default(DataModels.ItemListType);
            var listStatus = (DataModels.ItemListStatus)status;

            return list =>
                list.Status == listStatus
                &&
                (type.HasValue == false || list.ItemListType == listType);
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
                 Priority = l.Priority,
                 ItemListStatus = (ItemListStatus)l.Status,
                 ItemListType = (ItemListType)l.ItemListType,
                 ItemListRowsCount = l.ItemListRows.Count(),
                 ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                 CreationDate = l.CreationDate,
             }).AsNoTracking();
        }

        #endregion Methods
    }
}
