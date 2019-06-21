using System.Threading.Tasks;
using Ferretto.Common.DataModels;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSettingsController : BaseController
    {
        #region Fields

        private readonly IGlobalSettingsProvider globalSettingsProvider;

        #endregion

        #region Constructors

        public GlobalSettingsController(
            IGlobalSettingsProvider globalSettingsProvider)
        {
            this.globalSettingsProvider = globalSettingsProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(GlobalSettings), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<GlobalSettings>> GetAllAsync()
        {
            return this.Ok(
                await this.globalSettingsProvider.GetGlobalSettingsAsync());
        }

        #endregion
    }
}
