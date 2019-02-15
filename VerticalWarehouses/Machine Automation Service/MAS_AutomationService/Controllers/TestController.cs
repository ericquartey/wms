using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController
    {
        #region Fields

        private readonly IAutomationService automationService;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(
            IEventAggregator eventAggregator,
            IAutomationService automationService,
            IDataLayer dataLayer,
            IWriteLogService writeLogService)
        {
            this.eventAggregator = eventAggregator;
            this.automationService = automationService;
        }

        #endregion

        #region Methods

        [HttpGet("AddMissionTest")]
        public void AddMission()
        {
        }

        [HttpGet("HomingTest")]
        public string ExecuteHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteHoming));
            return "Execute Homing Done!";
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteStopHoming));
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.StopAction));
        }

        #endregion
    }
}
