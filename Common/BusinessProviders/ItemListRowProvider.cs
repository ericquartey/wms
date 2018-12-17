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

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public ItemListRowProvider(
            DatabaseContext dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
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

        public Task<ItemListRowDetails> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemListRow> GetByItemListId(int id)
        {
            lock (this.dataContext)
            {
                var itemListRows = this.dataContext.ItemListRows
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
                   ItemListRowStatus = (ItemListRowStatus)l.Status,
                   CreationDate = l.CreationDate
               }).AsNoTracking();

                return itemListRows;
            }
        }

        public int Save(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
