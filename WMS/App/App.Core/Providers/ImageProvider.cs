using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.App.Core.Providers
{
    public class ImageProvider : IFileProvider
    {
        #region Fields

        private const int defaultPixelMax = 600;

        private readonly Data.WebAPI.Contracts.IImagesDataService imageDataService;

        #endregion

        #region Constructors

        public ImageProvider(Data.WebAPI.Contracts.IImagesDataService imageDataService)
        {
            this.imageDataService = imageDataService;
        }

        #endregion

        #region Properties

        private static int DefaultPixelMax => int.TryParse(
            ConfigurationManager.AppSettings["ImagesDefaultPixelMax"], out var x) ?
            x :
            defaultPixelMax;

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

        public async Task<string> UploadAsync(string imagePath, IFormFile model)
        {
            if (imagePath == null)
            {
                return null;
            }

            var streamResized = ResizeImage(imagePath);
            ImageFile imageFile;
            if (streamResized != null)
            {
                imageFile = new ImageFile
                {
                    Stream = streamResized,
                    Length = streamResized.Length,
                    FileName = Path.GetFileName(imagePath),
                };
            }
            else
            {
                imageFile = new ImageFile
                {
                    Stream = new FileStream(imagePath, FileMode.Open),
                    Length = GetFileSize(imagePath),
                    FileName = Path.GetFileName(imagePath),
                };
            }

            var result = await this.imageDataService.UploadAsync(
               new Data.WebAPI.Contracts.FileParameter(imageFile.OpenReadStream(), imageFile.FileName));

            return result;
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
            return y * DefaultPixelMax / x;
        }

        private static Bitmap CreateResizedImage(System.Drawing.Image image, int width, int height)
        {
            if (image == null)
            {
                return null;
            }

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

        private static long GetFileSize(string filePath)
        {
            // if you don't have permission to the folder, Directory.Exists will return False
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // if you land here, it means you don't have permission to the folder
                Debug.Write("Permission denied");
                throw new FileLoadException();
            }

            return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        }

        private static ImageFormat GetImageFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException(
                    $"Unable to determine file extension for fileName: {fileName}");
            }

            switch (extension.ToLower())
            {
                case @".bmp":
                    return ImageFormat.Bmp;

                case @".gif":
                    return ImageFormat.Gif;

                case @".ico":
                    return ImageFormat.Icon;

                case @".jpg":
                case @".jpeg":
                    return ImageFormat.Jpeg;

                case @".png":
                    return ImageFormat.Png;

                case @".tif":
                case @".tiff":
                    return ImageFormat.Tiff;

                case @".wmf":
                    return ImageFormat.Wmf;

                default:
                    throw new NotImplementedException();
            }
        }

        private static Stream ResizeImage(string imagePath)
        {
            var stream = new MemoryStream();
            var format = GetImageFormat(imagePath);

            using (var image = Image.FromFile(imagePath))
            {
                if (image.Height <= DefaultPixelMax && image.Width <= DefaultPixelMax)
                {
                    return null;
                }

                var width = image.Width;
                var height = image.Height;
                CalculateDimensionProportioned(ref width, ref height);
                var resizedImage = CreateResizedImage(image, width, height);
                resizedImage.Save(stream, format);
                stream.Position = 0;
                return stream;
            }
        }

        #endregion
    }
}
