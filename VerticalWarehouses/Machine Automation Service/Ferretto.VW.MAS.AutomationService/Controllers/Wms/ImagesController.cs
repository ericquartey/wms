using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        #region Fields

        private readonly IImagesWmsWebService imagesWmsWebService;

        #endregion

        #region Constructors

        public ImagesController(IImagesWmsWebService imagesWmsWebService)
        {
            this.imagesWmsWebService = imagesWmsWebService ?? throw new ArgumentNullException(nameof(imagesWmsWebService));
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Download(string id)
        {
            var response = await this.imagesWmsWebService.DownloadAsync(id);

            return this.File(response.Stream, "image/jpeg", id);
        }

        #endregion
    }
}
