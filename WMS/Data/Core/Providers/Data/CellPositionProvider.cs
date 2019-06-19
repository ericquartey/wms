using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CellPositionProvider : BaseProvider, ICellPositionProvider
    {
        #region Constructors

        public CellPositionProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<CellPosition>> GetAllAsync()
        {
            return await this.DataContext.CellPositions
               .Select(c => new CellPosition
               {
                   Id = c.Id,
                   Description = c.Description,
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.CellPositions.CountAsync();
        }

        public async Task<CellPosition> GetByIdAsync(int id)
        {
            return await this.DataContext.CellPositions
                 .Select(c => new CellPosition
                 {
                     Id = c.Id,
                     Description = c.Description,
                 })
                 .SingleOrDefaultAsync(c => c.Id == id);
        }

        #endregion
    }
}
