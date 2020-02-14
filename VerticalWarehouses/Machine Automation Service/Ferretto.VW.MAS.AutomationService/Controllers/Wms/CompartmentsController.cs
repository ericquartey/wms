using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class CompartmentsController : ControllerBase
    {
        #region Fields

        private readonly ICompartmentsWmsWebService compartmentsWmsWebService;

        #endregion

        #region Constructors

        public CompartmentsController(ICompartmentsWmsWebService compartmentsWmsWebService)
        {
            this.compartmentsWmsWebService = compartmentsWmsWebService ?? throw new ArgumentNullException(nameof(compartmentsWmsWebService));
        }

        #endregion

        #region Methods

        public async Task<ActionResult<CompartmentDetails>> UpdateAsync(CompartmentDetails compartment, int id)
        {
            var updatedCompartment = await this.compartmentsWmsWebService.UpdateAsync(compartment, id);

            return this.Ok(updatedCompartment);
        }

        #endregion
    }
}
