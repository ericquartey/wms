using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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
    public class ResolutionCalibrationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ResolutionCalibrationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(bool))]
        [HttpPost("Completed")]
        public async Task<bool> CompletedAsync()
        {
            return await this.Completed_MethodAsync();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("Execute/{position}/{resolutionCalibrationSteps}")]
        public async Task<ActionResult> ExecuteAsync(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            return await this.ExecuteResolution_MethodAsync(position, resolutionCalibrationSteps);
        }

        [HttpGet("GetComputedResolution/{desiredDistance}/{desiredInitialPosition}/{desiredFinalPosition}/{resolution}")]
        public decimal GetComputedResolution(decimal desiredDistance, string desiredInitialPosition, string desiredFinalPosition, string resolution)
        {
            return this.GetComputedResolution_Method(desiredDistance, desiredInitialPosition, desiredFinalPosition, resolution);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetDecimalConfigurationParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("SetResolutionParameter/{newResolution}/")]
        public async Task<bool> SetResolutionParameterAsync(decimal newResolution)
        {
            return await this.SetResolutionParameter_MethodAsync(newResolution);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task<bool> Completed_MethodAsync()
        {
            var completionPersist = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetBoolConfigurationValueAsync((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus, true);
            }
            catch (Exception)
            {
                completionPersist = false;
            }

            return completionPersist;
        }

        private async Task<ActionResult> ExecuteResolution_MethodAsync(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            string message;

            var homingDone = await this.dataLayerConfigurationValueManagement.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);

            if (homingDone)
            {
                switch (resolutionCalibrationSteps)
                {
                    case ResolutionCalibrationSteps.StartProcedure:
                        message = "Resolution Calibration Start";
                        break;

                    case ResolutionCalibrationSteps.InitialPosition:
                        message = "Resolution Calibration go to initial position";
                        break;

                    case ResolutionCalibrationSteps.Move:
                        message = "Resolution Calibration move to final position";
                        break;

                    default:
                        message = string.Empty;
                        break;
                }

                try
                {
                    var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                        (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
                    var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                        (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
                    var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                        (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
                    var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                        (long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);
                    var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                        (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

                    var speed = maxSpeed * feedRate;
                    var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, position, speed, maxAcceleration, maxDeceleration, 0, 0, 0, resolution);
                    var commandMessage = new CommandMessage(
                        messageData,
                        message,
                        MessageActor.FiniteStateMachines,
                        MessageActor.WebApi,
                        MessageType.Positioning);
                    this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
                }
                catch (Exception ex)
                {
                    var msg = new NotificationMessage(
                        new WebApiExceptionMessageData(ex.Message, 0),
                        "WebApi Error",
                        MessageActor.Any,
                        MessageActor.WebApi,
                        MessageType.WebApiException,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                    return this.BadRequest();
                }
            }
            else
            {
                return this.UnprocessableEntity();
            }

            return this.Ok();
        }

        private decimal GetComputedResolution_Method(decimal desiredDistance, string desiredInitialPosition, string desiredFinalPosition, string resolution)
        {
            // TEMP: Is it better to compute the calculus inside the FSM ??
            var newResolution = 0m;

            if (decimal.TryParse(desiredInitialPosition, out var decDesiredInitialPosition) &&
                decimal.TryParse(desiredFinalPosition, out var decDesiredFinalPosition) &&
                decimal.TryParse(resolution, out var decResolution))
            {
                newResolution = decResolution * desiredDistance / (decDesiredFinalPosition - decDesiredInitialPosition);
            }

            return newResolution;
        }

        private async Task<ActionResult<decimal>> GetDecimalConfigurationParameter_MethodAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);

            switch (categoryId)
            {
                case ConfigurationCategory.VerticalAxis:

                    Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (verticalAxisParameterId != null)
                    {
                        decimal value1 = 0;

                        try
                        {
                            value1 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)verticalAxisParameterId, (long)categoryId);
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
                            value2 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)horizontalAxisParameterId, (long)categoryId);
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
                            value3 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)resolutionCalibrationParameterId, (long)categoryId);
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

        private async Task<bool> SetResolutionParameter_MethodAsync(decimal newResolution)
        {
            var resultAssignment = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis, newResolution);
            }
            catch (Exception)
            {
                resultAssignment = false;
            }

            return resultAssignment;
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
