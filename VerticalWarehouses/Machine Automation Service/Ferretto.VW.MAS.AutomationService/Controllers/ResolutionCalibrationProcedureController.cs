using System;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolutionCalibrationProcedureController : BaseAutomationController
    {

        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IResolutionCalibrationDataLayer resolutionCalibration;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public ResolutionCalibrationProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IResolutionCalibrationDataLayer resolutionCalibration,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            if (dataLayerConfigurationValueManagement is null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (resolutionCalibration is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibration));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.configurationProvider = dataLayerConfigurationValueManagement;
            this.resolutionCalibration = resolutionCalibration;
        }

        #endregion



        #region Methods

        [HttpGet("adjusted-resolution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetAdjustedResolution(decimal measuredDistance, decimal expectedDistance)
        {
            if (measuredDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.ResolutionCalibrationProcedure.MeasuredDistanceMustBeStrictlyPositive
                    });
            }

            if (expectedDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.ResolutionCalibrationProcedure.ExpectedDistanceMustBeStrictlyPositive
                    });
            }

            var resolution = this.configurationProvider.GetDecimalConfigurationValue(
                    VerticalAxis.Resolution,
                    ConfigurationCategory.VerticalAxis);

            return resolution * expectedDistance / measuredDistance;
        }

        [HttpGet("parameters")]
        public ActionResult<ResolutionCalibrationParameters> GetParameters()
        {
            var parameters = new ResolutionCalibrationParameters
            {
                CurrentResolution = this.configurationProvider.GetDecimalConfigurationValue(
                    VerticalAxis.Resolution,
                    ConfigurationCategory.VerticalAxis),

                InitialPosition = this.configurationProvider.GetDecimalConfigurationValue(
                    ResolutionCalibration.InitialPosition,
                    ConfigurationCategory.ResolutionCalibration),

                FinalPosition = this.configurationProvider.GetDecimalConfigurationValue(
                    ResolutionCalibration.FinalPosition,
                    ConfigurationCategory.ResolutionCalibration),
            };

            return this.Ok(parameters);
        }

        #endregion
    }
}
