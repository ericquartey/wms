using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ferretto.VW.App.Controls.Controls
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

        private const int ColorAlphaByteIndex = 3;

        private bool sourcePropertyChanging;

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

        #endregion

        #region Methods

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == Image.SourceProperty)
            {
                if (e.NewValue is ImageSource imageSource
                    &&
                    !this.sourcePropertyChanging)
                {
                    this.sourcePropertyChanging = true;

                    var newSource = this.ColorizeImage(imageSource);
                    this.Source = newSource;
                    this.sourcePropertyChanging = false;
                }
            }

            base.OnPropertyChanged(e);
        }

        private static void OnColorizeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcIcon wmsIcon
                &&
                !wmsIcon.sourcePropertyChanging
                &&
                e.NewValue is SolidColorBrush brush)
            {
                wmsIcon.sourcePropertyChanging = true;

                var newSource = wmsIcon.ColorizeImage(wmsIcon.Source);

                wmsIcon.Source = newSource;
                wmsIcon.sourcePropertyChanging = false;
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
