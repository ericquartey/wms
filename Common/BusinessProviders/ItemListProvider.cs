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
            return this.dataContext.Current.ItemLists
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
                   CreationDate = l.CreationDate
               }).AsNoTracking();
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
                var itemListDetails = dataContext.ItemLists
               .Include(l => l.ItemListStatus)
               .Include(l => l.ItemListType)
               .Include(l => l.ItemListRows)
               .Where(l => l.Id == id)
               .Select(l => new ItemListDetails
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
                   ItemListStatusId = l.ItemListStatusId,
                   ItemListTypeId = l.ItemListTypeId,
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

                itemListDetails.ItemListStatusChoices = this.enumerationProvider.GetAllItemListStatuses();
                itemListDetails.ItemListTypeChoices = this.enumerationProvider.GetAllItemListTypes();

                itemListDetails.ItemListRows = this.itemListRowProvider.GetByItemListById(id);

                //loadingUnitDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
                //foreach (var compartment in this.compartmentProvider.GetByLoadingUnitId(id))
                //{
                //    loadingUnitDetails.AddCompartment(compartment);
                //}

                //loadingUnitDetails.CellPairingChoices =
                //    ((DataModels.Pairing[])Enum.GetValues(typeof(DataModels.Pairing)))
                //    .Select(i => new Enumeration((int)i, i.ToString())).ToList();
                //loadingUnitDetails.ReferenceTypeChoices =
                //    ((DataModels.ReferenceType[])Enum.GetValues(typeof(DataModels.ReferenceType)))
                //    .Select(i => new EnumerationString(i.ToString(), i.ToString())).ToList();
                //loadingUnitDetails.CellChoices = this.cellProvider.GetByAreaId(loadingUnitDetails.AreaId);

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
