using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.Common.Controls
{
    public static class ImageUtils
    {
        #region Methods

        public static ImageSource RetrieveImage(IImageProvider imageService, string imagePath)
        {
            if (imageService == null)
            {
                throw new System.ArgumentNullException(nameof(imageService));
            }

            return !string.IsNullOrWhiteSpace(imagePath)
                ? GetImage(imageService, imagePath)
                : null;
        }

        private static ImageSource GetImage(IImageProvider imageService, string path)
        {
            using (var imageStream = imageService.GetImage(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = imageStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        #endregion
    }
}
