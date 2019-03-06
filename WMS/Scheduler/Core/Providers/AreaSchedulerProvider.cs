using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    public class AreaSchedulerProvider : IAreaSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public AreaSchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

        public async Task<Area> GetByIdAsync(int id)
        {
            return await this.databaseContext.Areas
               .Include(a => a.Bays)
               .ThenInclude(b => b.Missions)
               .Select(a => new Area
               {
                   Id = a.Id,
                   Bays = a.Bays.Select(b => new Bay
                   {
                       Id = b.Id,
                       LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                       LoadingUnitsBufferUsage = b.Missions.Count(m => m.Status != Common.DataModels.MissionStatus.Completed)
                   })
               })
               .SingleAsync(a => a.Id == id);
        }

        #endregion
    }
}
