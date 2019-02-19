using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class BayProvider : IBayProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly WMS.Data.WebAPI.Contracts.IBaysDataService baysDataService;

        #endregion

        #region Constructors

        public BayProvider(
            WMS.Data.WebAPI.Contracts.IBaysDataService baysDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.baysDataService = baysDataService;
            this.areasDataService = areasDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Bay>> GetAllAsync()
        {
            return (await this.baysDataService.GetAllAsync())
                .Select(b => new Bay
                {
                    Id = b.Id,
                    Description = b.Description,
                    LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                    BayTypeId = b.BayTypeId,
                    BayTypeDescription = b.BayTypeDescription,
                    AreaId = b.AreaId,
                    AreaName = b.AreaName,
                    MachineId = b.MachineId,
                    MachineNickname = b.MachineNickname,
                });
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.baysDataService.GetAllCountAsync();
        }

        public async Task<IEnumerable<Bay>> GetByAreaIdAsync(int id)
        {
            return (await this.areasDataService.GetBaysAsync(id))
                .Select(b => new Bay
                {
                    Id = b.Id,
                    Description = b.Description,
                    LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                    BayTypeId = b.BayTypeId,
                    BayTypeDescription = b.BayTypeDescription,
                    AreaId = b.AreaId,
                    AreaName = b.AreaName,
                    MachineId = b.MachineId,
                    MachineNickname = b.MachineNickname,
                });
        }

        public async Task<Bay> GetByIdAsync(int id)
        {
            var bay = await this.baysDataService.GetByIdAsync(id);
            return new Bay
            {
                Id = bay.Id,
                Description = bay.Description,
                LoadingUnitsBufferSize = bay.LoadingUnitsBufferSize,
                BayTypeId = bay.BayTypeId,
                BayTypeDescription = bay.BayTypeDescription,
                AreaId = bay.AreaId,
                AreaName = bay.AreaName,
                MachineId = bay.MachineId,
                MachineNickname = bay.MachineNickname,
            };
        }

        #endregion
    }
}
