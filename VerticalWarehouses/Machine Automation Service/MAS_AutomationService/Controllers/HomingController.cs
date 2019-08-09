using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class HomingController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200)]
        [HttpGet("Execute")]
        public void Execute()
        {
            this.ExecuteHoming_Method();
            this.Ok();
        }

        [ProducesResponseType(200)]
        [HttpGet("GetCurrentPositionAxis")]
        public void GetCurrentPositionAxis()
        {
            this.ExecuteGetCurrentPosition_Method();
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            return this.GetDecimalConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private void ExecuteGetCurrentPosition_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Sensors changed Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.SensorsChanged));
        }

        private void ExecuteHoming_Method()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    homingData,
                    "Execute Homing Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Homing));
        }

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);

            var category = (ConfigurationCategory)categoryId;
            switch (category)
            {
                case ConfigurationCategory.VerticalAxis:

                    Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (verticalAxisParameterId != null)
                    {
                        decimal value1 = 0;

                        try
                        {
                            value1 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)verticalAxisParameterId, category);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value1);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }

                case ConfigurationCategory.HorizontalAxis:

                    Enum.TryParse(typeof(HorizontalAxis), parameter, out var horizontalAxisParameterId);
                    if (horizontalAxisParameterId != null)
                    {
                        decimal value2 = 0;
                        try
                        {
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)horizontalAxisParameterId, category);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value2);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }
                case ConfigurationCategory.ResolutionCalibration:
                    Enum.TryParse(typeof(ResolutionCalibration), parameter, out var resolutionCalibrationParameterId);
                    if (resolutionCalibrationParameterId != null)
                    {
                        decimal value3 = 0;
                        try
                        {
                            value3 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)resolutionCalibrationParameterId, category);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value3);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }

                default:
                    break;
            }

            return 0;
        }

        private void Stop_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Stop Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Stop));
        }

        #endregion
    }
}
