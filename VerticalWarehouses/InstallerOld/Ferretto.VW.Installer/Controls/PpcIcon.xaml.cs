using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ferretto.VW.Installer.Controls
{
    public partial class PpcIcon : Image
    {
        #region Fields

        public static readonly DependencyProperty ColorizeBrushProperty = DependencyProperty.Register(
            nameof(ColorizeBrush),
            typeof(SolidColorBrush),
            typeof(PpcIcon),
            new PropertyMetadata(
                default(SolidColorBrush),
                new PropertyChangedCallback(OnColorizeBrushChanged)));

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(PpcIcon),
             new PropertyMetadata(
                null,
                new PropertyChangedCallback(OnSourceChanged)));

        private const int ColorAlphaByteIndex = 3;

        #endregion

        #region Constructors

        public PpcIcon()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public SolidColorBrush ColorizeBrush
        {
            get => (SolidColorBrush)this.GetValue(ColorizeBrushProperty);
            set => this.SetValue(ColorizeBrushProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        #endregion

        #region Methods

        protected static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcIcon ppcIcon)
            {
                ppcIcon.Source = ppcIcon.ColorizeImage(ppcIcon.ImageSource);
            }
        }

        private static void OnColorizeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcIcon ppcIcon)
            {
                ppcIcon.Source = ppcIcon.ColorizeImage(ppcIcon.ImageSource);
            }
        }

        private ImageSource ColorizeImage(ImageSource image)
        {
            if (this.ColorizeBrush == null || image is BitmapSource == false)
            {
                return image;
            }

            var bitmap = image as WriteableBitmap ?? new WriteableBitmap(image as BitmapSource);

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

        private void SetBaseSource(ImageSource imageSource)
        {
            base.Source = imageSource;
        }

        private byte[] TransformColor(byte[] currentPixel)
        {
            var targetColor = this.ColorizeBrush.Color;

            return new[]
            {
                targetColor.B,
                targetColor.G,
                targetColor.R,
                currentPixel[ColorAlphaByteIndex],
            };
        }

        #endregion
    }
}
