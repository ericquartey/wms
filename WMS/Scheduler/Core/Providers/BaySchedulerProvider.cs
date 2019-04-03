using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class BaySchedulerProvider : IBaySchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public BaySchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

        public async Task UpdatePriorityAsync(int id)
        {
            var bay = await this.databaseContext.Bays.SingleOrDefaultAsync(b => b.Id == id);
            if (bay == null)
            {
                throw new System.ArgumentException($"No bay with the id {id} exists", nameof(id));
            }

            bay.Priority++;
            this.databaseContext.Bays.Update(bay);

            await this.databaseContext.SaveChangesAsync();
        }

        #endregion
    }
}
