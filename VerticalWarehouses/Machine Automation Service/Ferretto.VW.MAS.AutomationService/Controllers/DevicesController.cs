using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IConfigurationProvider configurationProvider;

        private readonly IInverterProvider inverterProvider;

        private readonly IInverterProgrammingProvider inverterStateProvider;

        private readonly IIoDeviceProvider ioDeviceProvider;

        #endregion

        #region Constructors

        public DevicesController(
            IConfigurationProvider configurationProvider,
            IInverterProvider inverterProvider,
            IIoDeviceProvider ioDeviceProvider,
            IInverterProgrammingProvider inverterStateProvider)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.inverterProvider = inverterProvider;
            this.ioDeviceProvider = ioDeviceProvider;
            this.inverterStateProvider = inverterStateProvider;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

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

        [HttpGet("inverters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<Inverter>> GetInverters()
        {
            return this.Ok(this.inverterProvider.GetAllParameters());
        }

        [HttpGet("inverters/parameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<InverterParametersData>> GetParameters()
        {
            return this.Ok(this.inverterStateProvider.GetInvertersParametersData(this.inverterProvider.GetAllParameters()));
        }

        [HttpPost("inverters/program")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ProgramAllInverters(IEnumerable<Inverter> inverters)
        {
            this.inverterStateProvider.Start(inverters, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("inverter/program")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ProgramInverter(Inverter inverter)
        {
            this.inverterStateProvider.Start(inverter, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("inverters/read")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ReadAllInverters(IEnumerable<Inverter> inverters)
        {
            this.inverterStateProvider.Read(inverters, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("inverter/read")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ReadInverter(Inverter inverter)
        {
            this.inverterStateProvider.Read(inverter, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
