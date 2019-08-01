using System;
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
    internal class BayProvider : BaseProvider, IBayProvider
    {
        #region Fields

        internal static readonly System.Linq.Expressions.Expression<Func<Common.DataModels.Bay, BayAvailable>> SelectBay = (b) => new BayAvailable
        {
            Id = b.Id,
            LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
            LoadingUnitsBufferUsage = b.Missions.Count(
                       m => m.Operations.Any(o =>
                       o.Status != Common.DataModels.MissionOperationStatus.Completed
                       &&
                       o.Status != Common.DataModels.MissionOperationStatus.Incomplete)),
        };

        #endregion

        #region Constructors

        public BayProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Bay>> ActivateAsync(int id)
        {
            var bay = this.DataContext.Bays.Find(id);
            if (bay == null)
            {
                return new NotFoundOperationResult<Bay>();
            }

            bay.IsActive = true;

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushUpdate(new Bay { Id = id });
            }

            return new SuccessOperationResult<Bay>();
        }

        public async Task<IOperationResult<Bay>> DeactivateAsync(int id)
        {
            var bay = this.DataContext.Bays.Find(id);
            if (bay == null)
            {
                return new NotFoundOperationResult<Bay>();
            }

            bay.IsActive = false;

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushUpdate(new Bay { Id = id });
            }

            return new SuccessOperationResult<Bay>();
        }

        public async Task<IEnumerable<Bay>> GetAllAsync()
        {
            return await this.DataContext.Bays
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
            return await this.DataContext.Bays.CountAsync();
        }

        public async Task<IEnumerable<Bay>> GetByAreaIdAsync(int id)
        {
            return await this.DataContext.Bays
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
            return await this.DataContext.Bays
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

        public async Task<BayAvailable> GetByIdForExecutionAsync(int id)
        {
            return await this.DataContext.Bays
                .Select(SelectBay)
                .SingleOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bay>> GetByMachineIdAsync(int id)
        {
            return await this.DataContext.Bays
                             .Where(b => b.MachineId == id)
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

        public async Task<int> UpdatePriorityAsync(int id, int? increment)
        {
            var bay = await this.DataContext.Bays.SingleOrDefaultAsync(b => b.Id == id);
            if (bay == null)
            {
                throw new ArgumentException(
                    string.Format(
                        Resources.Errors.NoBayWithTheIdExists,
                        id),
                    nameof(id));
            }

            if (increment.HasValue)
            {
                bay.Priority += increment.Value;
            }
            else
            {
                bay.Priority++;
            }

            this.DataContext.Bays.Update(bay);

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushUpdate(new Bay { Id = id });
            }

            return bay.Priority;
        }

        #endregion
    }
}
