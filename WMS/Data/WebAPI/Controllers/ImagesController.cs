using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController :
        ControllerBase,
        IFileController
    {
        #region Fields

        private readonly IImageProvider imageProvider;

        #endregion

        #region Constructors

        public ImagesController(IImageProvider imageProvider)
        {
            this.imageProvider = imageProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(FileStreamResult))]
        [ProducesResponseType(400)]
        [HttpGet("{id}")]
        public ActionResult Download(string id)
        {
            if (id == null)
            {
                return this.BadRequest();
            }

            var imageFileInfo = this.imageProvider.GetById(id);
            if (imageFileInfo != null)
            {
                return this.File(imageFileInfo.Stream, imageFileInfo.ContentType, id);
            }

            return this.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
            });
        }

        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [HttpPost]
        public async Task<ActionResult<string>> UploadAsync(IFormFile model)
        {
            if (model == null || model.Length == 0)
            {
                return this.BadRequest();
            }

            try
            {
                var result = await this.imageProvider.CreateAsync(model);
                if (result != null)
                {
                    return this.Ok(result);
                }

                return this.UnprocessableEntity("Error");
            }
            catch (Exception ex)
            {
                return this.UnprocessableEntity(ex.Message);
            }
        }

        #endregion
    }
}
