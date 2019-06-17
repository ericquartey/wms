using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class BayProvider : IBayProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly WMS.Data.WebAPI.Contracts.IBaysDataService baysDataService;

        private readonly WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public BayProvider(
            WMS.Data.WebAPI.Contracts.IBaysDataService baysDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService,
            WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService)
        {
            this.baysDataService = baysDataService;
            this.areasDataService = areasDataService;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Bay>> GetAllAsync()
        {
            try
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
            catch
            {
                return new List<Bay>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.baysDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<IOperationResult<IEnumerable<Bay>>> GetByAreaIdAsync(int id)
        {
            try
            {
                var result = (await this.areasDataService.GetBaysAsync(id))
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

                return new OperationResult<IEnumerable<Bay>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Bay>>(e);
            }
        }

        public async Task<Bay> GetByIdAsync(int id)
        {
            try
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
            catch
            {
                return null;
            }
        }

        public async Task<IOperationResult<IEnumerable<BayDetails>>> GetByMachineIdAsync(int id)
        {
            try
            {
                var result = (await this.machinesDataService.GetBaysAsync(id))
                    .Select(b => new BayDetails
                    {
                        Id = b.Id,
                        Description = b.Description,
                        LoadingUnitsBufferSize = b.LoadingUnitsBufferSize,
                        BayTypeId = b.BayTypeId,
                        BayTypeDescription = b.BayTypeDescription,
                        AreaId = b.AreaId,
                        MachineId = b.MachineId,
                    });

                return new OperationResult<IEnumerable<BayDetails>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<BayDetails>>(e);
            }
        }

        #endregion
    }
}
