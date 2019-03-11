using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
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

        private readonly IPackageTypeProvider packageTypeProvider;

        private readonly ILogger logger;

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

        [ProducesResponseType(200, Type = typeof(IEnumerable<PackageType>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageType>>> GetAllAsync()
        {
            return this.Ok(await this.packageTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.packageTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(PackageType))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageType>> GetByIdAsync(int id)
        {
            var result = await this.packageTypeProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        #endregion
    }
}
