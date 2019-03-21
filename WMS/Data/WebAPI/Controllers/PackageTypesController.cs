using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class PackageTypesController :
        ControllerBase,
        IReadAllController<PackageType>,
        IReadSingleController<PackageType, int>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public PackageTypesController(
            ILogger<PackageTypesController> logger,
            IPackageTypeProvider packageTypeProvider)
        {
            this.logger = logger;
            this.packageTypeProvider = packageTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<PackageType>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageType>>> GetAllAsync()
        {
            return this.Ok(await this.packageTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.packageTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(PackageType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageType>> GetByIdAsync(int id)
        {
            var result = await this.packageTypeProvider.GetByIdAsync(id);
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
