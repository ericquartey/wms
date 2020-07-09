using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls
{
    internal sealed class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private const string BLANKIMAGE = "no_image.jpg";

        private readonly IMachineImagesWebService imagesWebService;

        #endregion

        #region Constructors

        public WmsImagesProvider(IMachineImagesWebService imagesWebService)
        {
            this.imagesWebService = imagesWebService ?? throw new ArgumentNullException(nameof(imagesWebService));
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
                using (var response = await this.imagesWebService.DownloadAsync(imageKey))
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
            try
            {
                var sri = Application.GetResourceStream(new Uri($"pack://application:,,,/{VW.Utils.Common.ASSEMBLY_MAINTHEMENAME};Component/Images/{BLANKIMAGE}"));
                return new MemoryStream(this.ReadFully(sri.Stream));
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
