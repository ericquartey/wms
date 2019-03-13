using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
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

        public async Task<IOperationResult<LoadingUnit>> UpdateAsync(LoadingUnit model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingModel = this.databaseContext.LoadingUnits.Find(model.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<LoadingUnit>(model);
        }

        #endregion
    }
}
