using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class AisleProvider : BaseProvider, IAisleProvider
    {
        #region Constructors

        public AisleProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Aisle>> GetAllAsync()
        {
            return await this.DataContext.Aisles
                       .Select(a => new Aisle
                       {
                           Id = a.Id,
                           Name = a.Name,
                           AreaId = a.AreaId,
                           AreaName = a.Area.Name,
                       })
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.Aisles.CountAsync();
        }

        public async Task<Aisle> GetByIdAsync(int id)
        {
            return await this.DataContext.Aisles
                       .Select(a => new Aisle
                       {
                           Id = a.Id,
                           Name = a.Name,
                           AreaId = a.AreaId,
                           AreaName = a.Area.Name,
                       })
                       .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
