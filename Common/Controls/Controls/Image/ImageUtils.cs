using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.Common.Controls
{
    public static class ImageUtils
    {
        #region Methods

        public static async Task<ImageSource> GetImageAsync(IFileProvider fileProvider, string path)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            if (path == null)
            {
                return null;
            }

            var imageFile = await fileProvider.DownloadAsync(path);

            var imageStream = imageFile.Stream;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        #endregion
    }
}
