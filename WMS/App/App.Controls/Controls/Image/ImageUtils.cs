using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.WMS.App.Controls
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

            var result = await fileProvider.DownloadAsync(path);
            if (result.Success == false)
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = result.Entity.Stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        #endregion
    }
}
