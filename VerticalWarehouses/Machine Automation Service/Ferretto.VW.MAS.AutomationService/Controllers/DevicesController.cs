using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        #region Fields

        private readonly IInverterProvider inverterProvider;

        private readonly IIoDeviceProvider ioDeviceProvider;

        #endregion

        #region Constructors

        public DevicesController(
            IInverterProvider inverterProvider,
            IIoDeviceProvider ioDeviceProvider)
        {
            this.inverterProvider = inverterProvider;
            this.ioDeviceProvider = ioDeviceProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public ActionResult<(IEnumerable<InverterDeviceInfo>, IEnumerable<IoDeviceInfo>)> GetAll()
        {
            var invertersStatuses = this.inverterProvider.GetStatuses;
            var ioDevicesStatuses = this.ioDeviceProvider.GetStatuses;
            var result = (InvertersStatuses: invertersStatuses, IoStatuses: ioDevicesStatuses);
            return this.Ok(result);
        }

        #endregion
    }
}
