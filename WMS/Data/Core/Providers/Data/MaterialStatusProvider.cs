using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MaterialStatusProvider : BaseProvider, IMaterialStatusProvider
    {
        #region Constructors

        public MaterialStatusProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<MaterialStatus>> GetAllAsync()
        {
            return await this.DataContext.MaterialStatuses
               .Select(c => new MaterialStatus
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.MaterialStatuses.CountAsync();
        }

        public async Task<MaterialStatus> GetByIdAsync(int id)
        {
            return await this.DataContext.MaterialStatuses
                 .Select(c => new MaterialStatus
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
