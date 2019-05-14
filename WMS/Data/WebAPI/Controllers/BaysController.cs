using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController :
        BaseController,
        IReadAllController<Bay>,
        IReadSingleController<Bay, int>
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ILogger logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public BaysController(
            ILogger<BaysController> logger,
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            IBayProvider bayProvider,
            IMachineProvider machineProvider)
            : base(hubContext)
        {
            this.logger = logger;
            this.bayProvider = bayProvider;
            this.machineProvider = machineProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<Bay>> ActivateAsync(int id)
        {
            var result = await this.bayProvider.ActivateAsync(id);
            if (result.Success == false)
            {
                if (result is NotFoundOperationResult<Bay>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult<Bay>> DeactivateAsync(int id)
        {
            var result = await this.bayProvider.DeactivateAsync(id);

            if (result.Success == false)
            {
                if (result is NotFoundOperationResult<Bay>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<Bay>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bay>>> GetAllAsync()
        {
            return this.Ok(await this.bayProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.bayProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(Machine), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/machine")]
        public async Task<ActionResult<Machine>> GetByBayIdAsync(int id)
        {
            var result = await this.machineProvider.GetByBayIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Bay>> GetByIdAsync(int id)
        {
            var result = await this.bayProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        #endregion
    }
}
