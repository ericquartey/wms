using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class PutToLightController : ControllerBase
    {
        #region Fields

        private readonly IPutToLightWmsWebService putToLightWmsWebService;

        #endregion

        #region Constructors

        public PutToLightController(IPutToLightWmsWebService putToLightWmsWebService)
        {
            this.putToLightWmsWebService = putToLightWmsWebService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("shelves/{shelfCode}/basket/{basketCode}")]
        public async Task<IActionResult> AssociateBasketToShelf(string basketCode, string shelfCode, int machineId, int bayNumber)
        {
            await this.putToLightWmsWebService.AssociateBasketToShelfAsync(basketCode, shelfCode, machineId, bayNumber);

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("shelves/{machineCode}/car-to-machine/{carCode}")]
        public async Task<IActionResult> CarToMachine(string carCode, string machineCode, int machineId, int bayNumber)
        {
            //await this.putToLightWmsWebService.CarToMachineAsync(carCode, machineCode, machineId, bayNumber);

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("shelves/{shelfCode}/complete-basket/{basketCode}")]
        public async Task<IActionResult> CompleteBasket(string basketCode, string shelfCode, int machineId, int bayNumber)
        {
            await this.putToLightWmsWebService.CompleteBasketAsync(basketCode, shelfCode, machineId, bayNumber);

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("shelves/{shelfCode}/full-basket/{basketCode}")]
        public async Task<IActionResult> RemoveFullBasket(string basketCode, string shelfCode, int machineId, int bayNumber)
        {
            await this.putToLightWmsWebService.RemoveFullBasketAsync(basketCode, shelfCode, machineId, bayNumber);

            return this.Ok();
        }

        #endregion
    }
}
