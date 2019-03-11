using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls.Services
{
    public class ImageProvider : IImageProvider
    {
        #region Fields

        private const string defaultImagesDirectoryName = "images\\";

        private const int defaultPixelMax = 600;

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

        private static Uri ImageDirectoryUri =>
                                    new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, ConfigurationManager.AppSettings["ImagesPath"] ?? defaultImagesDirectoryName));

        #endregion

        #region Methods

        public static Bitmap ResizeImage(Image image, int width, int height)
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

        public System.IO.Stream GetImage(string pathName)
        {
            if (string.IsNullOrWhiteSpace(pathName))
            {
                return null;
            }

            var uri = new Uri(ImageDirectoryUri, pathName);

            if (!ImageDirectoryUri.IsBaseOf(uri))
            {
                throw new ArgumentException(
                    Errors.SpecifiedPathNotInConfiguredImageFolder,
                    nameof(pathName));
            }

            return File.Exists(uri.LocalPath) ? File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read) : null;
        }

        public void SaveImage(string originalPathImage)
        {
            // Load image
            Image resizedImage = null;
            using (var image = Image.FromFile(originalPathImage))
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

                string fileName = Path.GetFileName(originalPathImage);

                var uri = new Uri(ImageDirectoryUri, fileName);
                var uriLocalPath = uri.LocalPath;
                if (File.Exists(uriLocalPath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPathImage);
                    fileNameWithoutExtension += $"_{DateTime.Now.Ticks}{Path.GetExtension(originalPathImage)}";
                    uri = new Uri(ImageDirectoryUri, fileNameWithoutExtension);
                }

                resizedImage.Save(uriLocalPath);
                resizedImage.Dispose();
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

        #endregion
    }
}
