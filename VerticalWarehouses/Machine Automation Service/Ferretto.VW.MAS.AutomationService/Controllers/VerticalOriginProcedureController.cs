using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerticalOriginProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        #endregion

        #region Constructors

        public VerticalOriginProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer configurationProvider)
            : base(eventAggregator)
        {
            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            this.configurationProvider = configurationProvider;
        }

        #endregion

        #region Methods

        [HttpGet("parameters")]
        public ActionResult<HomingProcedureParameters> GetParameters()
        {
            var category = ConfigurationCategory.VerticalAxis;

            var parameters = new HomingProcedureParameters
            {
                UpperBound = this.configurationProvider.GetDecimalConfigurationValue(VerticalAxis.UpperBound, category),
                LowerBound = this.configurationProvider.GetDecimalConfigurationValue(VerticalAxis.LowerBound, category),
                Offset = this.configurationProvider.GetDecimalConfigurationValue(VerticalAxis.Offset, category),
                Resolution = this.configurationProvider.GetDecimalConfigurationValue(VerticalAxis.Resolution, category),
            };

            return this.Ok(parameters);
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Start()
        {
            var homingData = new HomingMessageData(Axis.Both);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
