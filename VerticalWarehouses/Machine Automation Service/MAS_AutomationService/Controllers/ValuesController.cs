using System.Collections.Generic;
using Ferretto.VW.MAS_AutomationService;
using Prism.Events;
using Ferretto.Common.Common_Utils;
using Microsoft.AspNetCore.Mvc;
using Ferretto.VW.MAS_DataLayer;

namespace MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        #region Fields

        private readonly IAutomationService automationService;

        private readonly IWriteLogService log;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public ValuesController(IWriteLogService log, IAutomationService automationService, IEventAggregator eventAggregator)
        {
            this.log = log;
            this.automationService = automationService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("HomingTest")]
        public void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<TestHomingEvent>().Publish();
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            this.log.LogWriting("Called ValueController.Get()");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            this.log.LogWriting($"Called ValueController.Get({id})");

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        #endregion
    }
}
