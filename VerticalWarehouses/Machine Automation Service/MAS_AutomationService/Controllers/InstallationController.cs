using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer;
using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerValueManagment dataLayerValueManagement;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagement = services.GetService(typeof(IDataLayerValueManagment)) as IDataLayerValueManagment;
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

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("DecimalConfigurationParameter/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string parameter)
        {
            decimal returnValue;
            switch (parameter)
            {
                case nameof(ConfigurationValueEnum.UpperBound):
                    returnValue = this.dataLayerValueManagement.GetDecimalConfigurationValue((long)VerticalAxisEnum.UpperBound, (long)ConfigurationCategoryValueEnum.VerticalAxisEnum);
                    break;

                case nameof(ConfigurationValueEnum.LowerBound):
                    returnValue = this.dataLayerValueManagement.GetDecimalConfigurationValue((long)VerticalAxisEnum.LowerBound, (long)ConfigurationCategoryValueEnum.VerticalAxisEnum);
                    break;

                case nameof(ConfigurationValueEnum.Offset):
                    returnValue = this.dataLayerValueManagement.GetDecimalConfigurationValue((long)VerticalAxisEnum.Offset, (long)ConfigurationCategoryValueEnum.VerticalAxisEnum);
                    break;

                case nameof(ConfigurationValueEnum.Resolution):
                    returnValue = this.dataLayerValueManagement.GetDecimalConfigurationValue((long)VerticalAxisEnum.Resolution, (long)ConfigurationCategoryValueEnum.VerticalAxisEnum);
                    break;

                default:
                    var message = $"No entity with the specified parameter={parameter} exists.";
                    return this.NotFound(message);
            }
            return this.Ok(returnValue);
        }

        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Stop));
        }

        #endregion
    }
}
