using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAS_DataLayer;
using Microsoft.AspNetCore.Mvc;
using Ferretto.VW.MAS_AutomationService;

namespace MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        #region Fields

        private readonly IAutomationService automationService;

        private readonly IWriteLogService log;

        #endregion Fields

        #region Constructors

        public ValuesController(IWriteLogService log, IAutomationService automationService)
        {
            this.log = log;
            this.automationService = automationService;
        }

        #endregion Constructors

        #region Methods

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("HomingTest")]
        public void ExecuteHoming()
        {
            this.automationService.ExecuteHoming();
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

        #endregion Methods
    }
}
