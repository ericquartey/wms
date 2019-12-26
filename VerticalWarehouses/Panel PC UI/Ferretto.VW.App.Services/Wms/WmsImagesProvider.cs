using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private const string BLANKIMAGE = "no_image.jpg";

        private readonly IImagesWmsWebService imagesDataService;

        #endregion

        #region Constructors

        public WmsImagesProvider(IImagesWmsWebService imagesDataService)
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
            var sri = Application.GetResourceStream(new Uri($"pack://application:,,,/{VW.Utils.Common.ASSEMBLY_MAINTHEMENAME};Component/Images/{BLANKIMAGE}"));
            return new MemoryStream(this.ReadFully(sri.Stream));
        }

        #endregion
    }
}
