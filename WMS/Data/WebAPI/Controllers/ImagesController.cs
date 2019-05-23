using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult Download(string id)
        {
            if (id == null)
            {
                return this.BadRequest();
            }

            var result = this.imageProvider.GetById(id);
            if (result.Success)
            {
                var imageFileInfo = result.Entity;

                return this.File(imageFileInfo.Stream, imageFileInfo.ContentType, id);
            }

            return this.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
            });
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public async Task<ActionResult<string>> UploadAsync(IFormFile model)
        {
            if (model == null || model.Length <= 0)
            {
                return this.BadRequest();
            }

            var result = await this.imageProvider.CreateAsync(model);
            if (!result.Success)
            {
                return this.UnprocessableEntity(result.Description);
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
