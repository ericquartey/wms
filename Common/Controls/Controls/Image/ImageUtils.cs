using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Interfaces;

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
