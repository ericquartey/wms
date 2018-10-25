using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        #region Fields

        private readonly IHostingEnvironment env;

        #endregion Fields

        #region Constructors

        public FilesController(IHostingEnvironment env)
        {
            this.env = env;
        }

        #endregion Constructors

        #region Methods

        [HttpGet("{id}")]
        public IActionResult GetFile(string id)
        {
            var path = System.IO.Path.Combine(this.env.ContentRootPath, id);
            return this.PhysicalFile(path, "application/binary", id);
        }

        #endregion Methods
    }
}
