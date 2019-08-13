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
    public partial class ServiceController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ServiceController(
            IEventAggregator eventAggregator,
            IServiceProvider services,
            ILogger<ServiceController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.horizontalAxis = services.GetService(typeof(IHorizontalAxisDataLayer)) as IHorizontalAxisDataLayer;
            this.horizontalManualMovements = services.GetService(typeof(IHorizontalManualMovementsDataLayer)) as IHorizontalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200)]
        [HttpGet("ExecuteSearchHorizontalZero")]
        public void ExecuteSearchHorizontalZero()
        {
            this.ExecuteSearchHorizontalZero_Method();
        }

        #endregion
    }
}
