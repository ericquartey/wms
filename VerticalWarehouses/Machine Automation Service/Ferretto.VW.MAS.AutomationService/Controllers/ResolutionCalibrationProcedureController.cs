using System;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
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
    public class ResolutionCalibrationProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ResolutionCalibrationProcedureController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            if (elevatorDataProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorDataProvider));
            }

            if (dataLayerConfigurationValueManagement is null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (setupProceduresDataProvider is null)
            {
                throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.setupStatusProvider = setupStatusProvider;
            this.elevatorDataProvider = elevatorDataProvider;
            this.configurationProvider = dataLayerConfigurationValueManagement;
            this.setupProceduresDataProvider = setupProceduresDataProvider;
        }

        #endregion

        #region Methods

        [HttpGet("adjusted-resolution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetAdjustedResolution(double measuredDistance, double expectedDistance)
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

            var resolution = this.elevatorDataProvider.GetVerticalAxis().Resolution;

            return resolution * (decimal)expectedDistance / (decimal)measuredDistance;
        }

        [HttpGet("parameters")]
        public ActionResult<ResolutionCalibrationParameters> GetParameters()
        {
            var setupProcedures = this.setupProceduresDataProvider.GetAll();

            var parameters = new ResolutionCalibrationParameters
            {
                CurrentResolution = this.elevatorDataProvider.GetVerticalAxis().Resolution,
                InitialPosition = setupProcedures.VerticalResolutionCalibration.InitialPosition,
                FinalPosition = setupProcedures.VerticalResolutionCalibration.FinalPosition,
            };

            return this.Ok(parameters);
        }

        #endregion
    }
}
