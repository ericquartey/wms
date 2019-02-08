using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class BayProvider : IBayProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public BayProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Bay>> GetAllAsync()
        {
            return await this.dataContext.Bays
                             .Include(b => b.Area)
                             .Include(b => b.BayType)
                             .Include(b => b.Machine)
                             .Select(b => new Bay
                             {
                                 Id = b.Id,
                                 AreaId = b.AreaId,
                                 Description = b.Description,
                                 BayTypeId = b.BayTypeId,
                                 LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                                 MachineId = b.MachineId,
                                 AreaName = b.Area.Name,
                                 BayTypeDescription = b.BayType.Description,
                                 MachineNickname = b.Machine.Nickname,
                             })
                             .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.Bays.CountAsync();
        }

        public async Task<IEnumerable<Bay>> GetByAreaIdAsync(int id)
        {
            return await this.dataContext.Bays
                             .Include(b => b.BayType)
                             .Include(b => b.Area)
                             .Include(b => b.Machine)
                             .Where(b => b.AreaId == id)
                             .Select(b => new Bay
                             {
                                 Id = b.Id,
                                 Description = b.Description,
                                 LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                                 BayTypeId = b.BayTypeId,
                                 BayTypeDescription = b.BayType.Description,
                                 AreaId = b.AreaId,
                                 AreaName = b.Area.Name,
                                 MachineId = b.MachineId,
                                 MachineNickname = b.Machine.Nickname,
                             })
                             .ToArrayAsync();
        }

        public async Task<Bay> GetByIdAsync(int id)
        {
            return await this.dataContext.Bays
                             .Include(b => b.Area)
                             .Include(b => b.BayType)
                             .Include(b => b.Machine)
                             .Select(b => new Bay
                             {
                                 Id = b.Id,
                                 AreaId = b.AreaId,
                                 Description = b.Description,
                                 BayTypeId = b.BayTypeId,
                                 LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                                 MachineId = b.MachineId,
                                 AreaName = b.Area.Name,
                                 BayTypeDescription = b.BayType.Description,
                                 MachineNickname = b.Machine.Nickname,
                             })
                             .SingleOrDefaultAsync(b => b.Id == id);
        }

        #endregion
    }
}
