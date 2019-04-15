using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class BaySchedulerProvider : IBaySchedulerProvider
    {
        #region Fields

        internal static readonly System.Linq.Expressions.Expression<Func<Common.DataModels.Bay, Bay>> SelectBay = (b) => new Bay
        {
            Id = b.Id,
            LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
            LoadingUnitsBufferUsage = b.Missions.Count(
                       m => m.Status != Common.DataModels.MissionStatus.Completed
                       &&
                       m.Status != Common.DataModels.MissionStatus.Incomplete)
        };

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public BaySchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

        public async Task<Bay> GetByIdAsync(int id)
        {
            return await this.databaseContext.Bays
                .Select(SelectBay)
                .SingleOrDefaultAsync(b => b.Id == id);
        }

        public async Task<int> UpdatePriorityAsync(int id, int? increment)
        {
            var bay = await this.databaseContext.Bays.SingleOrDefaultAsync(b => b.Id == id);
            if (bay == null)
            {
                throw new System.ArgumentException($"No bay with the id {id} exists", nameof(id));
            }

            if (increment.HasValue)
            {
                bay.Priority += increment.Value + 1;
            }
            else
            {
                bay.Priority++;
            }

            this.databaseContext.Bays.Update(bay);

            await this.databaseContext.SaveChangesAsync();

            return bay.Priority;
        }

        #endregion
    }
}
