using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;

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
        [ProducesResponseType(422)]
        [HttpGet("{id}")]
        public async Task<ActionResult> DownloadAsync(string id)
        {
            if (id == null)
            {
                return this.BadRequest();
            }

            var fileImage = await this.imageProvider.DownloadAsync(id);
            await Task.Delay(8000);
            if (fileImage != null)
            {
                return this.File(fileImage.FileBytes, fileImage.ContentType, fileImage.FileName);
            }
            else
            {
                return this.UnprocessableEntity();
            }
        }

        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<string>> UploadAsync(IFormFile model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            return this.Ok(await this.imageProvider.UploadAsync(model));
        }

        #endregion
    }
}
