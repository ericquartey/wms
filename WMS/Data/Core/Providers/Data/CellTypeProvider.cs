using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CellTypeProvider : BaseProvider, ICellTypeProvider
    {
        #region Constructors

        public CellTypeProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<CellType>> GetAllAsync()
        {
            return await this.DataContext.CellTypes
               .Select(c => new CellType
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.CellTypes.CountAsync();
        }

        public async Task<CellType> GetByIdAsync(int id)
        {
            return await this.DataContext.CellTypes
                 .Select(c => new CellType
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(c => c.Id == id);
        }

        #endregion
    }
}
