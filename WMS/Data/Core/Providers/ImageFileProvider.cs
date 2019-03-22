using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Resources;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ImageFileProvider : IImageFileProvider
    {
        #region Fields

        private const string defaultImagesDirectoryName = "Images\\";

        private const int defaultPixelMax = 600;

        private const int MAX_SIZE_FILE = 1024;

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public ImageFileProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Properties

        private static int DefaultPixelMax
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["ImagesDefaultPixelMax"], out int x))
                {
                    return x;
                }

                return defaultPixelMax;
            }
        }

        private Uri ImageDirectoryUri =>
               new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, this.configuration.GetValue<string>("Image:Path") ?? defaultImagesDirectoryName));

        #endregion

        #region Methods

        public async Task<IImageFile> DownloadAsync(string key)
        {
            var path = System.IO.Path.Combine(this.ImageDirectoryUri.LocalPath, key);
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            var success = new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);
            var filename = Path.GetFileName(path);

            if (success)
            {
                return new ImageFile
                {
                    FileBytes = fileBytes,
                    ContentType = contentType,
                    FileName = filename,
                };
            }

            return null;
        }

        public ImageFile GetImage(string pathName)
        {
            if (string.IsNullOrWhiteSpace(pathName))
            {
                return null;
            }

            var uri = new Uri(this.ImageDirectoryUri, pathName);

            if (!this.ImageDirectoryUri.IsBaseOf(uri))
            {
                throw new ArgumentException(
                    Errors.SpecifiedPathNotInConfiguredImageFolder,
                    nameof(pathName));
            }

            var file = File.Exists(uri.LocalPath) ? File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read) : null;
            if (file != null)
            {
                var imageFile = new ImageFile
                {
                    FileName = uri.LocalPath,
                    Name = file.Name,
                    ContentType = file.GetType().ToString(),
                    Length = file.Length,
                };
                return imageFile;
            }

            return null;
        }

        public async Task<string> UploadAsync(IFormFile model)
        {
            // full path to file in temp location
            if (model == null)
            {
                return null;
            }

            var size = model.Length;
            if (size > MAX_SIZE_FILE)
            {
                // resize image
            }

            using (var memoryStream = new MemoryStream())
            {
                await model.OpenReadStream().CopyToAsync(memoryStream);
                return this.SaveImage(model, memoryStream);
            }
        }

        private static void CalculateDimensionProportioned(ref int width, ref int height)
        {
            if (width > height)
            {
                height = CalculateProportion(width, height);
                width = DefaultPixelMax;
            }
            else
            {
                width = CalculateProportion(height, width);
                height = DefaultPixelMax;
            }
        }

        private static int CalculateProportion(int x, int y)
        {
            return (y * DefaultPixelMax) / x;
        }

        private static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
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

        private string SaveImage(IFormFile model, Stream memoryStream)
        {
            // Load image
            Image resizedImage = null;
            using (var image = Image.FromStream(memoryStream))
            {
                if (image.Height > DefaultPixelMax || image.Width > DefaultPixelMax)
                {
                    var width = image.Width;
                    var height = image.Height;
                    CalculateDimensionProportioned(ref width, ref height);
                    resizedImage = ResizeImage(image, width, height);
                }
                else
                {
                    resizedImage = image;
                }

                var extension = Path.GetExtension(model.FileName);
                string fileName = Path.GetFileName($"{DateTime.Now.Ticks}{extension}");

                var uri = new Uri(this.ImageDirectoryUri, fileName);
                var uriLocalPath = uri.LocalPath;
                if (File.Exists(uriLocalPath))
                {
                    fileName = Path.GetFileName($"{DateTime.Now.Ticks}{DateTime.Now.Millisecond}{extension}");
                    uri = new Uri(this.ImageDirectoryUri, fileName);
                }

                resizedImage.Save(uriLocalPath);
                resizedImage.Dispose();
                return fileName;
            }
        }

        #endregion
    }
}
