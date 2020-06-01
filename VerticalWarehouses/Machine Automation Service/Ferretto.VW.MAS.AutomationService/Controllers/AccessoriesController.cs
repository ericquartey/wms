using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessoriesController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        #endregion

        #region Constructors

        public AccessoriesController(
            IBaysDataProvider baysDataProvider)
        {
            this.baysDataProvider = baysDataProvider;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<BayAccessories> GetAll()
        {
            var accessories = this.baysDataProvider.GetAccessories(this.BayNumber);

            return this.Ok(accessories);
        }

        [HttpPut("alpha-numeric-bar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateAlphaNumericBar(bool isEnabled, string ipAddress, int port)
        {
            this.baysDataProvider.SetAlphaNumericBar(this.BayNumber, isEnabled, ipAddress, port);

            return this.Ok();
        }

        [HttpPut("barcode-reader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateBarcodeReaderSettings(bool isEnabled, string portName)
        {
            this.baysDataProvider.UpdateBarcodeReaderSettings(this.BayNumber, isEnabled, portName);

            return this.Ok();
        }

        [HttpPut("card-reader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateCardReaderSettings(bool isEnabled, string tokenRegex)
        {
            this.baysDataProvider.UpdateCardReaderSettings(this.BayNumber, isEnabled, tokenRegex);

            return this.Ok();
        }

        [HttpPut("laser-pointer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateLaserPointer(bool isEnabled, string ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            this.baysDataProvider.SetLaserPointer(this.BayNumber, isEnabled, ipAddress, port, xOffset, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);

            return this.Ok();
        }

        #endregion
    }
}
