using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class ResolutionCalibrationController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ResolutionCalibrationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(bool))]
        [HttpPost("Completed")]
        public bool CompletedAsync()
        {
            return this.Completed_Method();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("Execute/{position}/{resolutionCalibrationSteps}")]
        public ActionResult ExecuteAsync(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            return this.ExecuteResolution_Method(position, resolutionCalibrationSteps);
        }

        [HttpGet("GetComputedResolution/{readDistance}/{desiredInitialPosition}/{desiredFinalPosition}/{resolution}")]
        public decimal GetComputedResolution(decimal readDistance, string desiredInitialPosition, string desiredFinalPosition, string resolution)
        {
            return this.GetComputedResolution_Method(readDistance, desiredInitialPosition, desiredFinalPosition, resolution);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            return this.GetDecimalConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("SetResolutionParameter/{newResolution}/")]
        public bool SetResolutionParameter(decimal newResolution)
        {
            return this.SetResolutionParameter_Method(newResolution);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private bool Completed_Method()
        {
            var completionPersist = true;

            try
            {
                this.dataLayerConfigurationValueManagement.SetBoolConfigurationValue((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus, true);
            }
            catch (Exception)
            {
                completionPersist = false;
            }

            return completionPersist;
        }

        private ActionResult ExecuteResolution_Method(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            string message;

            var homingDone = this.dataLayerConfigurationValueManagement.GetBoolConfigurationValue((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);

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
                    var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                        (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
                    var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                        (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
                    var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                        (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
                    var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                        (long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

                    var speed = maxSpeed * feedRate;
                    var messageData = new PositioningMessageData(
                        Axis.Vertical,
                        MovementType.Absolute,
                        position,
                        speed,
                        maxAcceleration,
                        maxDeceleration,
                        0,
                        0,
                        0);
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

        private decimal GetComputedResolution_Method(decimal readDistance, string desiredInitialPosition, string desiredFinalPosition, string resolution)
        {
            // TEMP: Is it better to compute the calculus inside the FSM ??
            var newResolution = 0m;

            if (decimal.TryParse(desiredInitialPosition, out var decDesiredInitialPosition) &&
                decimal.TryParse(desiredFinalPosition, out var decDesiredFinalPosition) &&
                decimal.TryParse(resolution, out var decResolution))
            {
                var desideredDistance = decDesiredFinalPosition - decDesiredInitialPosition;

                if (desideredDistance != 0)
                {
                    newResolution = decResolution * readDistance / desideredDistance;
                }
            }

            return newResolution;
        }

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string category, string parameter)
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
                            value1 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)verticalAxisParameterId, (long)categoryId);
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
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)horizontalAxisParameterId, (long)categoryId);
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
                            value3 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)resolutionCalibrationParameterId, (long)categoryId);
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

        private bool SetResolutionParameter_Method(decimal newResolution)
        {
            var resultAssignment = true;

            try
            {
                this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValue((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis, newResolution);
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
