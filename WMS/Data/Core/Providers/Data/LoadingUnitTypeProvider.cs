using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class LoadingUnitTypeProvider : BaseProvider, ILoadingUnitTypeProvider
    {
        #region Constructors

        public LoadingUnitTypeProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<LoadingUnitType>> GetAllAsync()
        {
            return await this.DataContext.LoadingUnitTypes
               .Select(c => new LoadingUnitType
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.LoadingUnitTypes.CountAsync();
        }

        public async Task<LoadingUnitType> GetByIdAsync(int id)
        {
            return await this.DataContext.LoadingUnitTypes
                 .Select(c => new LoadingUnitType
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(l => l.Id == id);
        }

        #endregion
    }
}
