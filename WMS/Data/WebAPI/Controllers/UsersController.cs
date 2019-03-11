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
    public class UsersController :
        ControllerBase,
        IReadAllController<User>,
        IReadSingleController<User, int>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IUserProvider userProvider;

        #endregion

        #region Constructors

        public UsersController(
            ILogger<UsersController> logger,
            IUserProvider userProvider)
        {
            this.logger = logger;
            this.userProvider = userProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllAsync()
        {
            return this.Ok(await this.userProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.userProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetByIdAsync(int id)
        {
            var result = await this.userProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(bool))]
        [HttpGet("is_valid")]
        public async Task<ActionResult<bool>> IsValidAsync(User user)
        {
            return this.Ok(await this.userProvider.IsValidAsync(user));
        }

        #endregion
    }
}
