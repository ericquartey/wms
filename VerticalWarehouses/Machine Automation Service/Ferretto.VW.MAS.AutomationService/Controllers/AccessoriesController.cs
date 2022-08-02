using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
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

        private readonly IPrintersWmsWebService printersWmsWebService;

        #endregion

        #region Constructors

        public AccessoriesController(IAccessoriesDataProvider accessoriesDataProvider,
            IPrintersWmsWebService printersWmsWebService)
        {
            this.accessoriesDataProvider = accessoriesDataProvider;
            this.printersWmsWebService = printersWmsWebService;
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

        [HttpGet("get-all-bay-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<BayAccessories> GetAllWithBayNumber(BayNumber BayNumber)
        {
            var accessories = this.accessoriesDataProvider.GetAccessories(BayNumber);

            return this.Ok(accessories);
        }

        [HttpPut("print-test-page")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PrintTestPageAsync(BayNumber BayNumber)
        {
            var printerName = this.accessoriesDataProvider.GetAccessories(BayNumber).LabelPrinter?.Name;
            if (string.IsNullOrEmpty(printerName))
            {
                return this.BadRequest();
            }
            await this.printersWmsWebService.PrintTestPageAsync(printerName);
            return this.Ok();
        }

        [HttpPut("alpha-numeric-bar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateAlphaNumericBar(bool isEnabled, string ipAddress, int port, AlphaNumericBarSize size, int maxMessageLength, bool clearOnClose)
        {
            this.accessoriesDataProvider.UpdateAlphaNumericBar(this.BayNumber, isEnabled, ipAddress, port, size, maxMessageLength, clearOnClose);

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

        [HttpPut("token-reader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateTokenReaderSettings(bool isEnabled, string portName)
        {
            this.accessoriesDataProvider.UpdateTokenReaderSettings(this.BayNumber, isEnabled, portName);

            return this.Ok();
        }

        [HttpPut("weighting-scale/info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateWeightingScaleDeviceInfo(DeviceInformation deviceInformation)
        {
            this.accessoriesDataProvider.UpdateWeightingScaleDeviceInfo(this.BayNumber, deviceInformation);

            return this.Ok();
        }

        [HttpPut("weighting-scale")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateWeightingScaleSettingsAsync(bool isEnabled, string ipAddress, int port, WeightingScaleModelNumber modelNumber)
        {
            this.accessoriesDataProvider.UpdateWeightingScaleSettings(this.BayNumber, isEnabled, ipAddress, port, modelNumber);

            return this.Ok();
        }

        #endregion
    }
}
