using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ferretto.Common.Controls
{
    internal class WmsIconViewModel : Prism.Mvvm.BindableBase
    {
        #region Fields

        private const int ColorAlphaByteIndex = 3;
        private SolidColorBrush colorizeBrush;
        private ImageSource source;

        #endregion Fields

        #region Properties

        public ImageSource Source
        {
            get => this.source;
            set => this.SetProperty(ref this.source, this.ColorizeImage(value));
        }

        #endregion Properties

        #region Methods

        public void ColorizeImage(SolidColorBrush brush)
        {
            this.colorizeBrush = brush;
            this.Source = this.ColorizeImage(this.Source);
        }

        public void RetrieveImage(string symbolName)
        {
            if (string.IsNullOrWhiteSpace(symbolName))
            {
                this.Source = null;
                return;
            }

            var sourcePath = Resources.Icons.ResourceManager.GetString(symbolName);

            if (sourcePath != null)
            {
                var bitmapImage = new BitmapImage(new Uri(sourcePath, UriKind.RelativeOrAbsolute));

                this.Source = this.ColorizeImage(bitmapImage);
            }
        }

        private ImageSource ColorizeImage(ImageSource image)
        {
            if (this.colorizeBrush == null || image is BitmapSource == false)
            {
                return image;
            }

            System.Diagnostics.Debug.WriteLine($"ColorizeImage: {this.colorizeBrush} - {this.source}");

            var bitmap = image as WriteableBitmap ?? new WriteableBitmap(image as BitmapImage);

            var currentPixel = new byte[bitmap.Format.BitsPerPixel / 8];

            for (var row = 0; row < bitmap.PixelHeight; row++)
            {
                for (var col = 0; col < bitmap.PixelWidth; col++)
                {
                    var cursor = new Int32Rect(col, row, 1, 1);
                    bitmap.CopyPixels(cursor, currentPixel, currentPixel.Length, 0);

                    var colorData = this.TransformColor(currentPixel);
                    bitmap.WritePixels(cursor, colorData, colorData.Length, 0);
                }
            }

            return bitmap;
        }

        private byte[] TransformColor(byte[] currentPixel)
        {
            var targetColor = this.colorizeBrush.Color;

            return new[]
            {
                targetColor.B,
                targetColor.G,
                targetColor.R,
                currentPixel[ColorAlphaByteIndex],
            };
        }

        #endregion Methods
    }
}
