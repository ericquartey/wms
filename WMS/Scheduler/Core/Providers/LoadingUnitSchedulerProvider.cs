using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class LoadingUnitSchedulerProvider : ILoadingUnitSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public LoadingUnitSchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

        public async Task<LoadingUnit> GetByIdAsync(int id)
        {
            return await this.databaseContext.LoadingUnits
               .Select(l => new LoadingUnit
               {
                   Id = l.Id,
                   LastPickDate = l.LastPickDate
               })
               .SingleOrDefaultAsync(l => l.Id == id);
        }

        #endregion
    }
}
