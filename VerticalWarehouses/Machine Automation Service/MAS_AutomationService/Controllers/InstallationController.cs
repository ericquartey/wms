using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpGet("ExecuteHoming")]
        public void ExecuteHoming()
        {
            ICalibrateMessageData homingData = new CalibrateMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(homingData, "Execute Homing Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Homing));
        }

        [HttpPost]
        [Route("ExecuteMovement")]
        public void ExecuteMovement([FromBody]MovementMessageDataDTO data)
        {
           var messageData = new MovementMessageData(data);
           this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Movement));
        }

        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public ActionResult<bool[]> GetInstallationStatus()
        {
            return this.StatusCode(500, "Not implemented yet");
            //TEMP bool[] installationStatus = DataLayer.GetInstallationStatus();
            // if (installationStatus != null)
            //{
            //    return this.Ok(installationStatus);
            //} else
            //{
            //    return StatusCode(500);
            //}
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("DecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            long.TryParse(parameter, out var parameterId);
            long.TryParse(category, out var categoryId);

            if (parameterId != 0)
            {
                decimal value;

                try
                {
                    value = await this.dataLayerValueManagement.GetDecimalConfigurationValueAsync(parameterId, categoryId);
                }
                catch (Exception)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Stop));
        }

        #endregion
    }
}
