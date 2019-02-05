using Ferretto.Common.Common_Utils;
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

        private readonly IWriteLogService log;

        #endregion

        #region Constructors

        public TestController(IWriteLogService log, IAutomationService automationService, IEventAggregator eventAggregator)
        {
            this.log = log;
            this.automationService = automationService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [HttpGet("HomingTest")]
        public void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<TestHomingEvent>().Publish();
        }

        #endregion
    }
}
