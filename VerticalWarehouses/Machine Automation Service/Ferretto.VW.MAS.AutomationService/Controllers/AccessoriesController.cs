using System.Threading.Tasks;
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

        private readonly IAccessoriesDataProvider accessoriesDataProvider;

        private readonly WMS.Data.WebAPI.Contracts.IPrintersWmsWebService printerWebService;

        #endregion

        #region Constructors

        public AccessoriesController(
            IAccessoriesDataProvider accessoriesDataProvider,
            WMS.Data.WebAPI.Contracts.IPrintersWmsWebService printerWebService)
        {
            this.accessoriesDataProvider = accessoriesDataProvider;
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
            var accessories = this.accessoriesDataProvider.GetAccessories(this.BayNumber);

            return this.Ok(accessories);
        }

        [HttpPost("print-test-page")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PrintTestPage(string printerName)
        {
            await this.printerWebService.PrintTestPageAsync(printerName);

            return this.Accepted();
        }

        [HttpPut("alpha-numeric-bar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateAlphaNumericBar(bool isEnabled, string ipAddress, int port)
        {
            this.accessoriesDataProvider.UpdateAlphaNumericBar(this.BayNumber, isEnabled, ipAddress, port);

            return this.Ok();
        }

        [HttpPut("barcode-reader/info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateBarcodeReaderDeviceInfo(DeviceInformation deviceInformation)
        {
            this.accessoriesDataProvider.UpdateBarcodeReaderDeviceInfo(this.BayNumber, deviceInformation);

            return this.Ok();
        }

        [HttpPut("barcode-reader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateBarcodeReaderSettings(bool isEnabled, string portName)
        {
            this.accessoriesDataProvider.UpdateBarcodeReaderSettings(this.BayNumber, isEnabled, portName);

            return this.Ok();
        }

        [HttpPut("card-reader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateCardReaderSettings(bool isEnabled, string tokenRegex)
        {
            this.accessoriesDataProvider.UpdateCardReaderSettings(this.BayNumber, isEnabled, tokenRegex);

            return this.Ok();
        }

        [HttpPut("laber-printer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateLabelPrinterSettings(bool isEnabled, string printerName)
        {
            this.accessoriesDataProvider.UpdateLabelPrinterSettings(this.BayNumber, isEnabled, printerName);

            return this.Ok();
        }

        [HttpPut("laser-pointer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateLaserPointer(bool isEnabled, string ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            this.accessoriesDataProvider.UpdateLaserPointer(this.BayNumber, isEnabled, ipAddress, port, xOffset, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);

            return this.Ok();
        }

        #endregion
    }
}
