using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    internal class WmsIconViewModel : BindableBase
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
            set => this.SetProperty(ref this.source, value);
        }

        #endregion Properties

        #region Methods

        public void ColorizeImage(SolidColorBrush brush)
        {
            this.colorizeBrush = brush;
            this.Source = this.ColorizeImageA(this.Source);
        }

        public void RetrieveImage(string symbolName)
        {
            var sourcePath = string.IsNullOrWhiteSpace(symbolName) ? null : Resources.Icons.ResourceManager.GetString(symbolName);

            var uri = new Uri(sourcePath, UriKind.RelativeOrAbsolute);

            var bitmapImage = new BitmapImage(uri);

            this.Source = this.ColorizeImageA(bitmapImage);
        }

        private ImageSource ColorizeImageA(ImageSource image)
        {
            if (this.colorizeBrush == null || image is BitmapImage == false)
            {
                return image;
            }

            var bitmap = new WriteableBitmap(image as BitmapImage);

            for (var row = 0; row < bitmap.Height; row++)
            {
                for (var col = 0; col < bitmap.Width; col++)
                {
                    var cursor = new Int32Rect(col, row, 1, 1);

                    var currentPixel = new byte[bitmap.Format.BitsPerPixel / 8];
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
