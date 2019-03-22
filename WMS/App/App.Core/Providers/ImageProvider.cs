using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.App.Core.Providers
{
    public class ImageProvider : IImageProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IImagesDataService imageDataService;

        #endregion

        #region Constructors

        public ImageProvider(WMS.Data.WebAPI.Contracts.IImagesDataService imageDataService)
        {
            this.imageDataService = imageDataService;
        }

        #endregion

        #region Methods

        public async Task<IImageFile> DownloadAsync(string key)
        {
            var fileResponse = await this.imageDataService.DownloadAsync(key);
            return new ImageFile
            {
                Stream = fileResponse.Stream,
            };
        }

        public async Task<string> UploadAsync(IFormFile model)
        {
            if (model == null)
            {
                return null;
            }

            return await this.imageDataService.UploadAsync(
               new WMS.Data.WebAPI.Contracts.FileParameter(model.OpenReadStream(), model.FileName));
        }

        #endregion
    }
}
