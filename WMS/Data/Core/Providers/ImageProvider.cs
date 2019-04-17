using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ImageProvider : IImageProvider
    {
        #region Fields

        private const string defaultImagesDirectoryName = "Images\\";

        private const int defaultPixelMax = 1024;

        private readonly IConfiguration configuration;

        private readonly IContentTypeProvider contentTypeProvider;

        private readonly IHostingEnvironment hostingEnvironment;

        #endregion

        #region Constructors

        public ImageProvider(
            IConfiguration configuration,
            IContentTypeProvider contentTypeProvider,
            IHostingEnvironment hostingEnvironment)
        {
            this.configuration = configuration;
            this.contentTypeProvider = contentTypeProvider;
            this.hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Properties

        private int DefaultPixelMax
        {
            get
            {
                if (int.TryParse(this.configuration.GetValue<string>("Image:DefaultPixelMax"), out var configValue))
                {
                    return configValue;
                }

                return defaultPixelMax;
            }
        }

        private string ImageVirtualPath =>
            this.configuration.GetValue<string>("Image:Path") ?? defaultImagesDirectoryName;

        #endregion

        #region Methods

        public async Task<IOperationResult<string>> CreateAsync(IFormFile model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.OpenReadStream().CopyToAsync(memoryStream);

                    var newFileName = this.SaveImage(model, memoryStream);
                    return new SuccessOperationResult<string>(newFileName);
                }
            }
            catch (Exception ex)
            {
                return new CreationErrorOperationResult<string>(ex);
            }
        }

        public ImageFile GetById(string key)
        {
            var path = Path.Combine(this.ImageVirtualPath, key);
            var success = this.contentTypeProvider.TryGetContentType(path, out var contentType);

            if (success && File.Exists(path))
            {
                return new ImageFile
                {
                    ContentType = contentType,
                    Stream = this.hostingEnvironment.ContentRootFileProvider.GetFileInfo(path).CreateReadStream(),
                    Path = path,
                };
            }

            return null;
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            if (image != null)
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }

            return null;
        }

        private void CalculateDimensionProportioned(ref int width, ref int height)
        {
            if (width > height)
            {
                height = this.CalculateProportion(width, height);
                width = this.DefaultPixelMax;
            }
            else
            {
                width = this.CalculateProportion(height, width);
                height = this.DefaultPixelMax;
            }
        }

        private int CalculateProportion(int x, int y)
        {
            return (y * this.DefaultPixelMax) / x;
        }

        private Image ResizeImage(Image image)
        {
            var resizedImage = image;
            if (image.Height > this.DefaultPixelMax || image.Width > this.DefaultPixelMax)
            {
                var width = image.Width;
                var height = image.Height;
                this.CalculateDimensionProportioned(ref width, ref height);
                resizedImage = ResizeImage(image, width, height);
            }

            return resizedImage;
        }

        private string SaveImage(IFormFile model, Stream memoryStream)
        {
            var absoluteFilePath = Path.Combine(
                this.hostingEnvironment.ContentRootPath,
                this.ImageVirtualPath);

            if (Directory.Exists(absoluteFilePath) == false)
            {
                Directory.CreateDirectory(absoluteFilePath);
            }

            using (var image = Image.FromStream(memoryStream))
            {
                var resizedImage = this.ResizeImage(image);

                var extension = Path.GetExtension(model.FileName);
                var fileName = Path.GetFileName($"{DateTime.Now.Ticks}{extension}");

                var imagePath = Path.Combine(absoluteFilePath, fileName);
                if (File.Exists(imagePath))
                {
                    fileName = Path.GetFileName($"{DateTime.Now.Ticks}{DateTime.Now.Millisecond}{extension}");
                    imagePath = Path.Combine(absoluteFilePath, fileName);
                }

                resizedImage.Save(imagePath);

                if (image.Equals(resizedImage) == false)
                {
                    resizedImage.Dispose();
                }

                return fileName;
            }
        }

        #endregion
    }
}
