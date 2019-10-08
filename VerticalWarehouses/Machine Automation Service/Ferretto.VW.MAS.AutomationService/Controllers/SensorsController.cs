using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : BaseAutomationController
    {
        #region Fields

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public SensorsController(
            IEventAggregator eventAggregator,
            ISensorsProvider sensorsProvider)
            : base(eventAggregator)
        {
            if (sensorsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(sensorsProvider));
            }

            this.sensorsProvider = sensorsProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<bool[]> Get()
        {
            return this.Ok(this.sensorsProvider.GetAll());
        }

        #endregion
    }
}
