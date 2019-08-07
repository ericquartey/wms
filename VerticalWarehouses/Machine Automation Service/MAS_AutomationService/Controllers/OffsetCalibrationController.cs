using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DataModels;
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
    public class OffsetCalibrationController : ControllerBase
    {
        #region Fields

        private readonly ICellManagmentDataLayer dataLayerCellsManagement;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public OffsetCalibrationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.dataLayerCellsManagement = services.GetService(typeof(ICellManagmentDataLayer)) as ICellManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteCompleted")]
        public bool ExecuteCompleted()
        {
            return this.ExecuteCompleted_Method();
        }

        [HttpGet("ExecutePositioning")]
        public void ExecutePositioning()
        {
            this.ExecutePositioning_Method();
        }

        [HttpGet("ExecuteStepDown")]
        public void ExecuteStepDown()
        {
            var stepValue = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

            this.ExecuteStep_Method(-stepValue);
        }

        [HttpGet("ExecuteStepUp")]
        public void ExecuteStepUp()
        {
            var stepValue = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

            this.ExecuteStep_Method(stepValue);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            return this.GetDecimalConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            return this.GetIntegerConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitPositionParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetLoadingUnitPositionParameter(string category, string parameter)
        {
            return this.GetLoadingUnitPositionParameter_Method(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitSideParameter/{category}/{parameter}")]
        public ActionResult<int> GetLoadingUnitSideParameter(string category, string parameter)
        {
            return this.GetLoadingUnitSideParameter_Method(category, parameter);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("SetOffsetParameter/{newOffset}/")]
        public bool SetOffsetParameter(decimal newOffset)
        {
            return this.SetOffsetParameter_Method(newOffset);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private bool ExecuteCompleted_Method()
        {
            var completionPersist = true;

            try
            {
                this.dataLayerConfigurationValueManagement.SetBoolConfigurationValue((long)SetupStatus.VerticalOffsetDone, ConfigurationCategory.SetupStatus, true);
            }
            catch (Exception)
            {
                completionPersist = false;
            }

            return completionPersist;
        }

        private void ExecutePositioning_Method()
        {
            var referenceCell = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue(
                (long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

            var position = 10; //TODO Retrieve the position related to the cellReference value

            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);
            var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);
            var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);
            var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                position,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private void ExecuteStep_Method(decimal displacement)
        {
            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);
            var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);
            var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);
            var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                displacement,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            var category = (ConfigurationCategory)categoryId;

            switch (categoryId)
            {
                case ConfigurationCategory.VerticalAxis:

                    Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (verticalAxisParameterId != null)
                    {
                        decimal value1 = 0;

                        try
                        {
                            value1 = this.dataLayerConfigurationValueManagement
                                .GetDecimalConfigurationValue(
                                (long)verticalAxisParameterId,
                                category);
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

                case ConfigurationCategory.OffsetCalibration:
                    Enum.TryParse(typeof(OffsetCalibration), parameter, out var offsetCalibrationParameterId);
                    if (offsetCalibrationParameterId != null)
                    {
                        decimal value2 = 0;
                        try
                        {
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)offsetCalibrationParameterId, category);
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

                default:
                    break;
            }

            return 0;
        }

        private ActionResult<int> GetIntegerConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            var category = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)parameterId, category);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
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

        private ActionResult<decimal> GetLoadingUnitPositionParameter_Method(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            if (parameterId != null)
            {
                LoadingUnitPosition value;
                var cellId = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

                try
                {
                    value = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value.LoadingUnitCoord);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        private ActionResult<int> GetLoadingUnitSideParameter_Method(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            if (parameterId != null)
            {
                LoadingUnitPosition value;
                var cellId = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

                try
                {
                    value = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok((int)value.LoadingUnitSide);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        private bool SetOffsetParameter_Method(decimal newOffset)
        {
            var resultAssignment = true;

            try
            {
                this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValue((long)VerticalAxis.Offset, ConfigurationCategory.VerticalAxis, newOffset);
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
