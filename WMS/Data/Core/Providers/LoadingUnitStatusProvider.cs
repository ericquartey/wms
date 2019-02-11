using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class LoadingUnitStatusProvider : ILoadingUnitStatusProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public LoadingUnitStatusProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<LoadingUnitStatus>> GetAllAsync()
        {
            return await this.dataContext.LoadingUnitStatuses
               .Select(c => new LoadingUnitStatus
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.LoadingUnitStatuses.CountAsync();
        }

        public async Task<LoadingUnitStatus> GetByIdAsync(string id)
        {
            return await this.dataContext.LoadingUnitStatuses
                 .Select(c => new LoadingUnitStatus
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(l => l.Id == id);
        }

        #endregion
    }
}
