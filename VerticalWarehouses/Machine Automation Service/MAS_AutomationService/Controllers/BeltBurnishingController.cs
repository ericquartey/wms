using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class BeltBurnishingController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public BeltBurnishingController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("Execute/{upperBound}/{lowerBound}/{requiredCycles}")]
        public async Task Execute(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            await this.ExecuteBeltBurnishing_Method(upperBound, lowerBound, requiredCycles);
        }

        private async Task ExecuteBeltBurnishing_Method(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var acceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var deceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                upperBound,
                maxSpeed,
                acceleration,
                deceleration,
                requiredCycles,
                lowerBound,
                upperBound,
                resolution);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    positioningMessageData,
                    "Execute Belt Burnishing Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));
        }

        #endregion
    }
}
