using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class CompartmentStatusProvider : BaseProvider, ICompartmentStatusProvider
    {
        #region Constructors

        public CompartmentStatusProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<CompartmentStatus>> GetAllAsync()
        {
            return await this.DataContext.CompartmentStatuses
               .Select(c => new CompartmentStatus
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.CompartmentStatuses.CountAsync();
        }

        public async Task<CompartmentStatus> GetByIdAsync(int id)
        {
            return await this.DataContext.CompartmentStatuses
                 .Select(c => new CompartmentStatus
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
