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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        private readonly ILogger<ImageProvider> logger;

        #endregion

        #region Constructors

        public ImageProvider(
            IConfiguration configuration,
            IContentTypeProvider contentTypeProvider,
            IHostingEnvironment hostingEnvironment,
            ILogger<ImageProvider> logger)
        {
            this.configuration = configuration;
            this.contentTypeProvider = contentTypeProvider;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }

        #endregion

        #region Properties

        private int DefaultPixelMax =>
            int.TryParse(this.configuration.GetValue<string>("Image:DefaultPixelMax"), out var configValue) ?
                configValue :
                defaultPixelMax;

        private string ImageVirtualPath =>
            this.configuration.GetValue<string>("Image:Path") ?? defaultImagesDirectoryName;

        #endregion

        #region Methods

        public IOperationResult<string> Create(string uploadImageName, byte[] uploadImageData)
        {
            if (uploadImageName == null)
            {
                throw new ArgumentNullException(nameof(uploadImageName));
            }

            if (uploadImageData == null)
            {
                throw new ArgumentNullException(nameof(uploadImageData));
            }

            try
            {
                using (var memoryStream = new MemoryStream(uploadImageData))
                {
                    var newFileName = this.SaveImage(uploadImageName, memoryStream);
                    return new SuccessOperationResult<string>(newFileName);
                }
            }
            catch (Exception ex)
            {
                return new CreationErrorOperationResult<string>(ex);
            }
        }

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

                    var newFileName = this.SaveImage(model.FileName, memoryStream);
                    return new SuccessOperationResult<string>(newFileName);
                }
            }
            catch (Exception ex)
            {
                return new CreationErrorOperationResult<string>(ex);
            }
        }

        public IOperationResult<ImageFile> GetById(string key)
        {
            var path = Path.Combine(this.ImageVirtualPath, key);
            var file = this.hostingEnvironment.ContentRootFileProvider.GetFileInfo(path);

            if (!file.Exists)
            {
                this.logger.LogWarning($"The requested file '{file.PhysicalPath}' does not exist.");

                return new NotFoundOperationResult<ImageFile>();
            }

            var success = this.contentTypeProvider.TryGetContentType(file.PhysicalPath, out var contentType);
            if (success)
            {
                return new SuccessOperationResult<ImageFile>(new ImageFile
                {
                    ContentType = contentType,
                    Stream = file.CreateReadStream(),
                    Path = path,
                });
            }
            else
            {
                this.logger.LogWarning($"Could not get content type for file '{file.PhysicalPath}'.");

                return new UnprocessableEntityOperationResult<ImageFile>(
                    $"Could not get content type for file {key}");
            }
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

        private string SaveImage(string originalFileName, Stream memoryStream)
        {
            var absoluteFilePath = Path.Combine(
                this.hostingEnvironment.ContentRootPath,
                this.ImageVirtualPath);

            if (!Directory.Exists(absoluteFilePath))
            {
                Directory.CreateDirectory(absoluteFilePath);
            }

            using (var image = Image.FromStream(memoryStream))
            {
                var resizedImage = this.ResizeImage(image);

                var extension = Path.GetExtension(originalFileName);
                var fileName = Path.GetFileName($"{DateTime.Now.Ticks}{extension}");

                var imagePath = Path.Combine(absoluteFilePath, fileName);
                if (File.Exists(imagePath))
                {
                    fileName = Path.GetFileName($"{DateTime.Now.Ticks}{DateTime.Now.Millisecond}{extension}");
                    imagePath = Path.Combine(absoluteFilePath, fileName);
                }

                resizedImage.Save(imagePath);

                if (!image.Equals(resizedImage))
                {
                    resizedImage.Dispose();
                }

                return fileName;
            }
        }

        #endregion
    }
}
