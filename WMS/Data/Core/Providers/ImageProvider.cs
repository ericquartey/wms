using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ImageProvider : IImageProvider
    {
        #region Fields

        private const string defaultImagesDirectoryName = "Images\\";

        private const int defaultPixelMax = 1024;

        private readonly IConfiguration configuration;

        private readonly IFileProvider fileProvider;

        private readonly IContentTypeProvider contentTypeProvider;

        #endregion

        #region Constructors

        public ImageProvider(IConfiguration configuration, IFileProvider fileProvider, IContentTypeProvider contentTypeProvider)
        {
            this.configuration = configuration;
            this.fileProvider = fileProvider;
            this.contentTypeProvider = contentTypeProvider;
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

        public ImageFile GetById(string key)
        {
            var path = Path.Combine(this.ImageVirtualPath, key);
            var success = this.contentTypeProvider.TryGetContentType(path, out var contentType);

            if (success)
            {
                return new ImageFile
                {
                    ContentType = contentType,
                    Stream = this.fileProvider.GetFileInfo(path).CreateReadStream(),
                    Path = path,
                };
            }

            return null;
        }

        public async Task<string> CreateAsync(IFormFile model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var memoryStream = new MemoryStream())
            {
                await model.OpenReadStream().CopyToAsync(memoryStream);
                return this.SaveImage(model, memoryStream);
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

        private string SaveImage(IFormFile model, Stream memoryStream)
        {
            using (var image = Image.FromStream(memoryStream))
            {
                var resizedImage = image;
                var toBeResized = image.Height > this.DefaultPixelMax || image.Width > this.DefaultPixelMax;
                if (toBeResized)
                {
                    var width = image.Width;
                    var height = image.Height;
                    this.CalculateDimensionProportioned(ref width, ref height);
                    resizedImage = ResizeImage(image, width, height);
                }

                var extension = Path.GetExtension(model.FileName);
                var fileName = Path.GetFileName($"{DateTime.Now.Ticks}{extension}");

                var imagePath = Path.Combine(this.ImageVirtualPath, fileName);
                if (File.Exists(imagePath))
                {
                    fileName = Path.GetFileName($"{DateTime.Now.Ticks}{DateTime.Now.Millisecond}{extension}");
                    imagePath = Path.Combine(this.ImageVirtualPath, fileName);
                }

                resizedImage.Save(imagePath);
                if (toBeResized)
                {
                    resizedImage.Dispose();
                }

                return fileName;
            }
        }

        #endregion
    }
}
