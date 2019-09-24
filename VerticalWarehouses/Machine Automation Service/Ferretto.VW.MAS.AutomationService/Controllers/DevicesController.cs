using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : BaseAutomationController
    {
        #region Fields

        private readonly IInverterProvider inverterProvider;

        private readonly IIoDeviceProvider ioDeviceProvider;

        #endregion

        #region Constructors

        public DevicesController(IEventAggregator eventAggregator,
                                 IInverterProvider inverterProvider,
                                 IIoDeviceProvider ioDeviceProvider)
                : base(eventAggregator)
        {
            this.inverterProvider = inverterProvider;
            this.ioDeviceProvider = ioDeviceProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<(IEnumerable<InverterDevice>, IEnumerable<IoDevice>)> GetAll()
        {
            try
            {
                var inverterStatuses = this.inverterProvider.GetStatuses;
                var ioDeviceStatuses = this.ioDeviceProvider.GetStatuses;
                var result = (InvertersStatuses: inverterStatuses, IoStatuses: ioDeviceStatuses);
                return this.Ok(result);
            }
            catch (System.Exception ex)
            {
                return this.BadRequest();
            }
        }

        #endregion
    }
}
