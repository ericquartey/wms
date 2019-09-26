using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

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

        public DevicesController(IEventAggregator eventAggregator,
                                 IInverterProvider inverterProvider,
                                 IIoDeviceProvider ioDeviceProvider)
        {
            this.inverterProvider = inverterProvider;
            this.ioDeviceProvider = ioDeviceProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<(IEnumerable<InverterDevice>, IEnumerable<IoDevice>)> GetAll()
        {
                var invertersStatuses = this.inverterProvider.GetStatuses;
                var ioDevicesStatuses = this.ioDeviceProvider.GetStatuses;
                var result = (InvertersStatuses: invertersStatuses, IoStatuses: ioDevicesStatuses);
                return this.Ok(result);
        }

        #endregion
    }
}
