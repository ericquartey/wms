using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public class PpcImage : System.Windows.Controls.Image
    {
        #region Fields

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            nameof(Path),
            typeof(string),
            typeof(PpcImage),
            new PropertyMetadata(
                default(string),
                async (d, e) =>
                {
                    if (d is PpcImage image)
                    {
                        image.Source = await image.GetImageAsync((string)e.NewValue)
                            .ConfigureAwait(true);
                    }
                }));

        private readonly IWmsImagesProvider imageService;

        #endregion

        #region Constructors

        public PpcImage()
        {
            this.imageService = ServiceLocator.Current.GetInstance<IWmsImagesProvider>();
        }

        #endregion

        #region Properties

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion

        #region Methods

        public async Task<ImageSource> GetImageAsync(string path)
        {
            var stream = await this.imageService.GetImageAsync(path);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        #endregion
    }
}
