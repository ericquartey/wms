using System.Collections.Generic;
using Ferretto.Common.BusinessModels;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : ControllerBase
    {
        #region Methods

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Machine>> Get()
        {
            return new Machine[]
            {
                new Machine
                {
                    Id = 1,
                    AisleName = "Vertimag 1",
                    MachineTypeDescription = "Vertimag",
                    LastPowerOn = System.DateTime.Now,
                    Model = "2018/XS"
                },
                new Machine
                {
                    Id = 2,
                    AisleName = "Vertimag 2",
                    MachineTypeDescription = "Vertimag",
                    LastPowerOn = System.DateTime.Now.Subtract(System.TimeSpan.FromMinutes(15)),
                    Model = "2018/XS"
                },
            };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
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
