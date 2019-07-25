using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Test/[controller]")]
    [ApiController]
    public partial class TestController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagment;

        private readonly IEventAggregator eventAggregator;

        private IDataHubClient dataHubClient;

        private IHubContext<InstallationHub, IInstallationHub> installationHub;

        private IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        public TestController(
            IEventAggregator eventAggregator,
            IServiceProvider services,
            IHubContext<InstallationHub, IInstallationHub> hub,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IDataHubClient dataHubClient)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagment = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.installationHub = hub;
            this.dataHubClient = dataHubClient;
            this.operatorHub = operatorHub;
        }

        #endregion

        #region Methods

        [HttpGet("BayNowServiceable")]
        public void BayNowServiceable()
        {
            this.BayNowServiceableMethod();
        }

        [HttpGet("HomingTest")]
        public async void ExecuteHoming()
        {
            await this.ExecuteHomingMethod();
        }

        [HttpPost]
        [Route("ExecuteResolutionCalibration/{readInitialPosition}/{readFinalPosition}")]
        public async Task ExecuteResolutionCalibrationAsync(decimal readInitialPosition, decimal readFinalPosition)
        {
            await this.ExecuteResolutionCalibrationMethod(readInitialPosition, readFinalPosition);
        }

        [HttpPost]
        public async Task ExecuteShutterPositioningMovementTestAsync([FromBody]ShutterPositioningMovementMessageDataDto data)
        {
            await this.ExecuteShutterPositioningMovementMethod();
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.ExecuteStopHomingMethod();
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("DecimalConfigurationValues/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string parameter)
        {
            return this.GetDecimalConfigurationParameterMethod(parameter);
        }

        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public ActionResult<bool[]> GetInstallationStatus()
        {
            return this.GetInstallationStatusMethod();
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            return this.GetIntegerConfigurationParameterMethod(category, parameter);
        }

        [HttpGet("Homing")]
        public async void Homing()
        {
            this.HomingMethod();
        }

        [HttpGet("HorizontalPositioning")]
        public void HorizontalPositioning()
        {
            this.HorizontalPositioningMethod();
        }

        [HttpGet("MissionExecutedTest")]
        public void MissionExecuted()
        {
        }

        [HttpGet("ResetIO")]
        public void ResetIO()
        {
        }

        [HttpGet("StartShutterControl/{bayNumber}/{delay}/{numberCycles}")]
        public async Task StartShutterControlAsync(int bayNumber, int delay, int numberCycles)
        {
            await this.StartShutterControlMethod();
        }

        [HttpGet("StartShutterControlError/{delay}/{numberCycles}")]
        public void StartShutterControlError(int delay, int numberCycles)
        {
            this.StartShutterControlErrorMethod(delay, numberCycles);
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.StopFiniteStateMachineMethod();
        }

        [HttpGet("UpdateCurrentPositionTest")]
        public async Task UpdateCurrentPositionTest()
        {
            await this.UpdateCurrentPositionTestMethod();
        }

        [HttpGet("VerticalPositioning")]
        public void VerticalPositioning()
        {
            this.VerticalPositioningMethod();
        }

        #endregion
    }
}
