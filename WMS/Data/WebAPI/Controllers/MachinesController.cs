using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController :
        BaseController,
        IReadAllPagedController<Machine>,
        IReadSingleController<MachineDetails, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ILogger logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionProvider missionProvider;

        #endregion

        #region Constructors

        public MachinesController(
            ILogger<MachinesController> logger,
            IMachineProvider machineProvider,
            IMissionProvider missionProvider,
            ILoadingUnitProvider loadingUnitProvider,
            IBayProvider bayProvider)
        {
            this.logger = logger;
            this.machineProvider = machineProvider;
            this.missionProvider = missionProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<Machine>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Machine>>> GetAllAsync(
            int skip = 0,
            int take = 0,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.machineProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.machineProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<Bay>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/bays")]
        public async Task<ActionResult<IEnumerable<Bay>>> GetBaysAsync(int id)
        {
            var bays = await this.bayProvider.GetByMachineIdAsync(id);

            if (!bays.Any())
            {
                var message = $"No entity associated with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(bays);
        }

        [ProducesResponseType(typeof(MachineDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MachineDetails>> GetByIdAsync(int id)
        {
            var result = await this.machineProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = string.Format(Resources.Errors.NoEntityExists, id);
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnitDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/loading-units")]
        public async Task<ActionResult<IEnumerable<LoadingUnitDetails>>> GetLoadingUnitsByIdAsync(int id)
        {
            var loadingUnits = await this.loadingUnitProvider.GetAllByMachineIdAsync(id);

            return this.Ok(loadingUnits);
        }

        [ProducesResponseType(typeof(IEnumerable<MissionInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/missions")]
        public async Task<ActionResult<IEnumerable<MissionInfo>>> GetMissionsByIdAsync(int id)
        {
            var result = await this.missionProvider.GetByMachineIdAsync(id);
            return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.machineProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        #endregion
    }
}
