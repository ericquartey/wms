using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public partial class MachineServiceController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public MachineServiceController(IEventAggregator eventAggregator, IServiceProvider services, ILogger<MachineServiceController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.horizontalAxis = services.GetService(typeof(IHorizontalAxisDataLayer)) as IHorizontalAxisDataLayer;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200)]
        [HttpGet("ExecuteSearchHorizontalZero/{speed}")]
        public void ExecuteSearchHorizontalZero(decimal speed)
        {
            this.ExecuteSearchHorizontalZero_Method(speed);
        }

        #endregion
    }
}
