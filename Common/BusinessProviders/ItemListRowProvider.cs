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

        public Task<Int32> Add(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemListRow> GetAll()
        {
            throw new NotImplementedException();
        }

        public Int32 GetAllCount()
        {
            throw new NotImplementedException();
        }

        public ItemListRowDetails GetById(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemListRow> GetByItemListId(Int32 id)
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
                   RowPriority = l.RowPriority,
                   ItemDescription = l.Item.Description,
                   RequiredQuantity = l.RequiredQuantity,
                   ItemListRowStatus = (ItemListRowStatus)l.ItemListRowStatus,
                   CreationDate = l.CreationDate
               }).AsNoTracking();

                return itemListRows;
            }
        }

        public Int32 Save(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
