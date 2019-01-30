namespace Ferretto.Common.Modules.BLL.Services
{
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

    public class ImageProvider : IImageProvider
    {
        #region Fields

        private const string defaultImagesDirectoryName = "images\\";

        private const int PixelMax = 600;

        #endregion Fields

        #region Properties

        private static Uri ImageDirectoryUri =>
                            new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, ConfigurationManager.AppSettings["ImagesPath"] ?? defaultImagesDirectoryName));

        #endregion Properties

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

        public void SaveImage(string pathImage)
        {
            // Load image
            var image = Image.FromFile(pathImage);

            if (image.Height > PixelMax || image.Width > PixelMax)
            {
                var width = image.Width;
                var height = image.Height;
                CalculateDimensionProportioned(ref width, ref height);
                image = ResizeImage(image, width, height);
            }

            string fileName = Path.GetFileName(pathImage);

            var uri = new Uri(ImageDirectoryUri, fileName);

            if (File.Exists(uri.LocalPath))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathImage);
                fileNameWithoutExtension += $"_{DateTime.Now.Ticks}{Path.GetExtension(pathImage)}";
                uri = new Uri(ImageDirectoryUri, fileNameWithoutExtension);
            }

            SaveOnLocal(image, uri.LocalPath);
        }

        private static void CalculateDimensionProportioned(ref int width, ref int height)
        {
            if (width > height)
            {
                height = CalculateProportion(width, height);
                width = PixelMax;
            }
            else
            {
                width = CalculateProportion(height, width);
                height = PixelMax;
            }
        }

        private static int CalculateProportion(int x, int y)
        {
            return (y * PixelMax) / x;
        }

        private static void SaveOnLocal(Image image, string path)
        {
            image.Save(path);
            image.Dispose();
        }

        #endregion Methods
    }
}
