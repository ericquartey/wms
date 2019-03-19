using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class BayProvider : IBayProvider
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

        public async Task<IOperationResult<Bay>> ActivateAsync(int id)
        {
            var bay = this.dataContext.Bays.Find(id);
            if (bay == null)
            {
                return new NotFoundOperationResult<Bay>();
            }

            bay.IsActive = true;

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<Bay>();
        }

        public async Task<IOperationResult<Bay>> DeactivateAsync(int id)
        {
            var bay = this.dataContext.Bays.Find(id);
            if (bay == null)
            {
                return new NotFoundOperationResult<Bay>();
            }

            bay.IsActive = false;

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<Bay>();
        }

        public async Task<IEnumerable<Bay>> GetAllAsync()
        {
            return await this.dataContext.Bays
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
