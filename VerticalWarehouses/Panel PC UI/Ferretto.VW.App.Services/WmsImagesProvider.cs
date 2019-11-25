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

        public async Task<Stream> GetImageAsync(string id)
        {
            Stream imageStream = null;
            if (id != null)
            {
                try
                {
                    using (var response = await this.imagesDataService.DownloadAsync(id))
                    {
                        imageStream = new MemoryStream(this.ReadFully(response.Stream));
                    }
                }
                catch (Exception)
                {
                    imageStream = this.LoadFallbackImage();
                }
            }
            else
            {
                imageStream = this.LoadFallbackImage();
            }

            return imageStream;
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
            var stream = new FileStream("./Images/Divieto_Dark.png", FileMode.Open);

            return new MemoryStream(this.ReadFully(stream));
        }

        #endregion
    }
}
