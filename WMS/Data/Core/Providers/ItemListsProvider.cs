using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ItemListsProvider : IItemListsProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemListsProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<ItemList>> GetAllAsync()
        {
            return await this.dataContext.ItemLists
                             .AsNoTracking()
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
                             })
                             .ToArrayAsync();
        }

        public async Task<ItemList> GetByIdAsync(int id)
        {
            return await this.dataContext.ItemLists
                             .AsNoTracking()
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
                             })
                             .SingleOrDefaultAsync(l => l.Id == id);
        }

        #endregion
    }
}
