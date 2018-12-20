using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion Fields

        #region Constructors

        public ItemListRowProvider(
            IDatabaseContextService dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> Add(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemListRow> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            throw new NotImplementedException();
        }

        public async Task<ItemListRowDetails> GetById(int id)
        {
            var itemListRowDetails = await this.dataContext.Current.ItemListRows
                .Include(lr => lr.ItemList)
                .Where(lr => lr.Id == id)
                .Select(lr => new ItemListRowDetails
                {
                    Id = lr.Id,
                    Code = lr.Code,
                    RowPriority = lr.Priority,
                    ItemDescription = lr.Item.Description,
                    RequiredQuantity = lr.RequiredQuantity,
                    DispatchedQuantity = lr.DispatchedQuantity,
                    ItemListRowStatus = (ItemListRowStatus) lr.Status,
                    CreationDate = lr.CreationDate,
                    ItemListCode = lr.ItemList.Code,
                    ItemListDescription = lr.ItemList.Description,
                    ItemListType = ((ItemListType)lr.ItemList.ItemListType),
                    ItemListStatus = ((ItemListStatus)lr.ItemList.Status),
                }).SingleAsync();

            return itemListRowDetails;
        }

        public IQueryable<ItemListRow> GetByItemListId(int id)
        {
            var itemListRows = this.dataContext.Current.ItemListRows
                .Include(l => l.MaterialStatus)
                .Include(l => l.Item)
                .Where(l => l.ItemListId == id)
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.Priority,
                    ItemDescription = l.Item.Description,
                    RequiredQuantity = l.RequiredQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListRowStatus = (ItemListRowStatus) l.Status,
                    CreationDate = l.CreationDate
                }).AsNoTracking();

            return itemListRows;
        }

        public int Save(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
