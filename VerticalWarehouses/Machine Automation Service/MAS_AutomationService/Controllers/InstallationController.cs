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
        [HttpGet("DecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            if (!Enum.TryParse(category, false, out ConfigurationCategory configurationCategory))
            {
                return this.NotFound($"No configuration category found for {category} value");
            }

            string errorMessage = string.Empty;
            long parameterId = 0;
            switch (configurationCategory)
            {
                case ConfigurationCategory.GeneralInfo:
                    if (!Enum.TryParse(parameter, false, out GeneralInfo generalInfoData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)generalInfoData;
                    }

                    break;

                case ConfigurationCategory.SetupNetwork:
                    if (!Enum.TryParse(parameter, false, out SetupNetwork setupNetworkData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)setupNetworkData;
                    }

                    break;

                case ConfigurationCategory.SetupStatus:
                    if (!Enum.TryParse(parameter, false, out SetupStatus setupStatusData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)setupStatusData;
                    }

                    break;

                case ConfigurationCategory.VerticalAxis:
                    if (!Enum.TryParse(parameter, false, out VerticalAxis verticalAxisData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)verticalAxisData;
                    }

                    break;

                case ConfigurationCategory.HorizontalAxis:
                    if (!Enum.TryParse(parameter, false, out HorizontalAxis horizontalAxisData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)horizontalAxisData;
                    }

                    break;

                case ConfigurationCategory.HorizontalMovementForwardProfile:
                    if (!Enum.TryParse(parameter, false, out HorizontalMovementForwardProfile horizontalMovementForwardProfileData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)horizontalMovementForwardProfileData;
                    }

                    break;

                case ConfigurationCategory.HorizontalMovementBackwardProfile:
                    if (!Enum.TryParse(parameter, false, out HorizontalMovementBackwardProfile horizontalMovementBackwardProfileData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)horizontalMovementBackwardProfileData;
                    }

                    break;

                case ConfigurationCategory.VerticalManualMovements:
                    if (!Enum.TryParse(parameter, false, out VerticalManualMovements verticalManualMovementsData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)verticalManualMovementsData;
                    }

                    break;

                case ConfigurationCategory.HorizontalManualMovements:
                    if (!Enum.TryParse(parameter, false, out HorizontalManualMovements horizontalManualMovementsData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)horizontalManualMovementsData;
                    }

                    break;

                case ConfigurationCategory.BeltBurnishing:
                    if (!Enum.TryParse(parameter, false, out BeltBurnishing beltBurnishingData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)beltBurnishingData;
                    }

                    break;

                case ConfigurationCategory.ResolutionCalibration:
                    if (!Enum.TryParse(parameter, false, out ResolutionCalibration resolutionCalibrationData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)resolutionCalibrationData;
                    }

                    break;

                case ConfigurationCategory.OffsetCalibration:
                    if (!Enum.TryParse(parameter, false, out OffsetCalibration offsetCalibrationData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)offsetCalibrationData;
                    }

                    break;

                case ConfigurationCategory.CellControl:
                    if (!Enum.TryParse(parameter, false, out CellControl cellControlData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)cellControlData;
                    }

                    break;

                case ConfigurationCategory.PanelControl:
                    if (!Enum.TryParse(parameter, false, out PanelControl panelControlData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)panelControlData;
                    }

                    break;

                case ConfigurationCategory.ShutterHeightControl:
                    if (!Enum.TryParse(parameter, false, out ShutterHeightControl shutterHeightControlData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)shutterHeightControlData;
                    }

                    break;

                case ConfigurationCategory.WeightControl:
                    if (!Enum.TryParse(parameter, false, out WeightControl weightControlData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)weightControlData;
                    }

                    break;

                case ConfigurationCategory.BayPositionControl:
                    if (!Enum.TryParse(parameter, false, out BayPositionControl bayPositionControlData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)bayPositionControlData;
                    }

                    break;

                case ConfigurationCategory.LoadFirstDrawer:
                    if (!Enum.TryParse(parameter, false, out LoadFirstDrawer loadFirstDrawerData))
                    {
                        errorMessage = $"No configuration value {parameter} found in category {category}";
                    }
                    else
                    {
                        parameterId = (long)loadFirstDrawerData;
                    }

                    break;
            }

            if (parameterId != 0)
            {
                return this.Ok(this.dataLayerValueManagement.GetDecimalConfigurationValue(parameterId, (long)configurationCategory));
            }
            else
            {
                return this.NotFound(errorMessage);
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
