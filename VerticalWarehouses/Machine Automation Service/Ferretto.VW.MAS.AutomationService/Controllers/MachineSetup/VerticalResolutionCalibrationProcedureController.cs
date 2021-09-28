using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class VerticalResolutionCalibrationProcedureController : ControllerBase
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationProcedureController(
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet("adjusted-resolution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<double> GetAdjustedResolution(double measuredDistance, double expectedDistance)
        {
            if (measuredDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("DataEntryError", CommonUtils.Culture.Actual),
                        Detail = Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("MeasuredDistanceMustBeStrictlyPositive", CommonUtils.Culture.Actual),
                    });
            }

            if (expectedDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("DataEntryError", CommonUtils.Culture.Actual),
                        Detail = Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("ExpectedDistanceMustBeStrictlyPositive", CommonUtils.Culture.Actual),
                    });
            }

            var resolution = this.elevatorDataProvider.GetAxis(Orientation.Vertical).Resolution;

            return resolution * expectedDistance / measuredDistance;
        }

        [HttpGet("parameters")]
        public ActionResult<VerticalResolutionCalibrationProcedure> GetParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetVerticalResolutionCalibration());
        }

        #endregion
    }
}
