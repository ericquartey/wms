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

namespace Ferretto.Common.Controls
{
    public class ImageUtils
    {
        #region Methods

        public static async Task<ImageSource> GetImageAsync(IImageProvider imageService, string path)
        {
            var imageFile = await imageService.DownloadAsync(path);

            var imageStream = imageFile.Stream;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public static async Task<ImageSource> RetrieveImageAsync(IImageProvider imageService, string imagePath)
        {
            return !string.IsNullOrWhiteSpace(imagePath)
                ? await ImageUtils.GetImageAsync(imageService, imagePath)
                : null;
        }

        #endregion
    }
}
