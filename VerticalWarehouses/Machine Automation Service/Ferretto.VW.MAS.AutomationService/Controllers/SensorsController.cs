using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        #region Fields

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public SensorsController(ISensorsProvider sensorsProvider)
        {
            this.sensorsProvider = sensorsProvider ?? throw new System.ArgumentNullException(nameof(sensorsProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<bool[]> Get()
        {
            return this.Ok(this.sensorsProvider.GetAll());
        }

        [HttpGet("out-fault")]
        public ActionResult<bool[]> GetOutFault()
        {
            return this.Ok(this.sensorsProvider.GetOutFault());
        }

        [HttpGet("out-current")]
        public ActionResult<int[]> GetOutCurrent()
        {
            return this.Ok(this.sensorsProvider.GetOutCurrent());
        }

        #endregion
    }
}
