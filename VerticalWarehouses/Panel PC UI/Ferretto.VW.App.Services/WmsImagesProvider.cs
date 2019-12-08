using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private readonly IImagesDataService imagesDataService;

        #endregion

        #region Constructors

        public WmsImagesProvider(IImagesDataService imagesDataService)
        {
            if (imagesDataService is null)
            {
                throw new ArgumentNullException(nameof(imagesDataService));
            }

            this.imagesDataService = imagesDataService;
        }

        #endregion

        #region Methods

        public async Task<Stream> GetImageAsync(string imageKey)
        {
            if (imageKey is null)
            {
                return this.LoadFallbackImage();
            }

            try
            {
                using (var response = await this.imagesDataService.DownloadAsync(imageKey))
                {
                    return new MemoryStream(this.ReadFully(response.Stream));
                }
            }
            catch (Exception)
            {
                return this.LoadFallbackImage();
            }
        }

        public byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private Stream LoadFallbackImage()
        {
            var stream = new FileStream("./Images/no_image.jpg", FileMode.Open);

            return new MemoryStream(this.ReadFully(stream));
        }

        #endregion
    }
}
