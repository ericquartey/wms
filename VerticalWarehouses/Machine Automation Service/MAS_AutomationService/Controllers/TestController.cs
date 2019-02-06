using Microsoft.AspNetCore.Mvc;
using Ferretto.Common.Common_Utils;
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

        #endregion Fields

        #region Constructors

        public TestController(IEventAggregator eventAggregator, IAutomationService automationService)
        {
            this.eventAggregator = eventAggregator;
            this.automationService = automationService;
        }

        #endregion Constructors

        #region Methods

        [HttpGet("AddMissionTest")]
        public void AddMission()
        {
        }

        [HttpGet("HomingTest")]
        public void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_ExecuteActionEvent>().Publish(WebAPI_Action.VerticalHoming);
        }

        #endregion Methods
    }
}
