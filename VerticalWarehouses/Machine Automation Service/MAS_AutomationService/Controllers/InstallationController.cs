using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public partial class InstallationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly ISetupStatus dataLayerSetupStatus;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.dataLayerSetupStatus = services.GetService(typeof(ISetupStatus)) as ISetupStatus;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        public async Task<bool> AcceptNewDecResolutionCalibration(decimal newDecResolution)
        {
            return await this.AcceptNewDecResolutionCalibrationMethod(newDecResolution);
        }

        //[HttpPost("ExecuteBeltBurnishing/{upperBound}/{lowerBound}/{requiredCycles}")]
        //public async Task ExecuteBeltBurnishing(decimal upperBound, decimal lowerBound, int requiredCycles)
        //{
        //    await this.ExecuteBeltBurnishingMethod(upperBound, lowerBound, requiredCycles);
        //}

        //[HttpGet("ExecuteHoming")]
        //public void ExecuteHoming()
        //{
        //    this.ExecuteHomingMethod();
        //}

        //[HttpPost("ExecuteMovement")]
        //public async Task ExecuteMovement([FromBody]MovementMessageDataDTO data)
        //{
        //    await this.ExecuteMovementMethod(data);
        //}

        [HttpPost("ExecuteResolution/{position}/{resolutionCalibrationSteps}")]
        public async Task ExecuteResolution(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            await this.ExecuteResolutionMethod(position, resolutionCalibrationSteps);
        }

        [HttpPost]
        [Route("ExecuteResolutionCalibration/{readInitialPosition}/{readFinalPosition}")]
        public void ExecuteResolutionCalibration(decimal readInitialPosition, decimal readFinalPosition)
        {
            this.ExecuteResolutionCalibrationMethod(readInitialPosition, readFinalPosition);
        }

        [HttpGet("ExecuteSensorsChangedCommand")]
        public void ExecuteSensorsChanged()
        {
            this.ExecuteSensorsChangedMethod();
        }

        //[HttpPost("ExecuteShutterPositioningMovement")]
        //public async Task ExecuteShutterPositioningMovementAsync([FromBody]ShutterPositioningMovementMessageDataDTO data)
        //{
        //    await this.ExecuteShutterPositioningMovementMethod(data);
        //}

        [HttpGet("ExecuteVerticalOffsetCalibration")]
        public async Task ExecuteVerticalOffsetCalibration()
        {
            await this.ExecuteVerticalOffsetCalibrationMethod();
        }

        [HttpGet("GetComputedResolutionCalibration/{desiredDistance}/{desiredInitialPosition}/{desiredFinalPosition}/{resolution}")]
        public decimal GetComputedResolutionCalibration(decimal desiredDistance, string desiredInitialPosition, string desiredFinalPosition, string resolution)
        {
            return this.GetComputedResolutionCalibrationMethod(desiredDistance, desiredInitialPosition, desiredFinalPosition, resolution);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetDecimalConfigurationParameterMethod(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(bool[]))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public async Task<ActionResult<bool[]>> GetInstallationStatus()
        {
            return await this.GetInstallationStatusMethod();
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetIntegerConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetIntegerConfigurationParameterMethod(category, parameter);
        }

        [HttpPost]
        [Route("LSM-HorizontalAxis/{Displacement}")]
        public async Task HorizontalAxisForLSM(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            this.HorizontalAxisForLSMMethod(displacement, axis, movementType, speedPercentage);
        }

        [HttpPost]
        [Route("ResolutionCalibrationComplete")]
        public async Task<bool> ResolutionCalibrationComplete()
        {
            return await this.ResolutionCalibrationCompleteMethod();
        }

        [HttpPost]
        [Route("SetDecimalConfigurationParameter/{category}/{parameter}/{value}")]
        public async Task SetDecimalConfigurationParameterAsync(string category, string parameter, decimal value)
        {
            await this.SetDecimalConfigurationParameterMethod(category, parameter, value);
        }

        [HttpPost]
        [Route("SetIntegerConfigurationParameter/{category}/{parameter}/{value}")]
        public async Task SetIntegerConfigurationParameterAsync(string category, string parameter, int value)
        {
            await this.SetIntegerConfigurationParameterMethod(category, parameter, value);
        }

        [HttpPost]
        [Route("LSM-ShutterPositioning/{shutterMovementDirection}")]
        public async Task ShutterPositioningForLSM(int bayNumber, decimal speedRate)
        {
            this.ShutterPositioningForLSMMethod(bayNumber, speedRate);
        }

        //[HttpGet("StartShutterControl/{bayNumber}/{delay}/{numberCycles}")]
        //public async Task StartShutterControlAsync(int bayNumber, int delay, int numberCycles)
        //{
        //    this.StartShutterControlMethod(bayNumber, delay, numberCycles);
        //}

        //[ProducesResponseType(200)]
        //[HttpGet("StopCommand")]
        //public void StopCommand()
        //{
        //    this.StopCommandMethod();
        //}

        [HttpPost]
        [Route("LSM-VerticalAxis/{Displacement}")]
        public async Task VerticalAxisForLSM(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            this.VerticalAxisForLSMMethod(displacement, axis, movementType, speedPercentage);
        }

        #endregion
    }
}
