using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls
{
    public class ImageUtils
    {
        #region Fields

        private static ImageSource placeholderImage = null;

        #endregion Fields

        #region Methods

        public static ImageSource GetImage(IImageProvider imageService, string path)
        {
            using (Stream imageStream = imageService.GetImage(path))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = imageStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        public static ImageSource RetrieveImage(IImageProvider imageService, string imagePath)
        {
            return !string.IsNullOrWhiteSpace(imagePath)
                ? ImageUtils.GetImage(imageService, imagePath)
                : null;
        }

        public static ImageSource RetrieveImageOrPlaceholder(IImageProvider imageService, string imagePath)
        {
            if (placeholderImage == null)
            {
                placeholderImage = ImageUtils.GetImage(imageService, Resources.Icons.PlaceHolder);
            }

            return !string.IsNullOrWhiteSpace(imagePath)
                ? placeholderImage
                : ImageUtils.GetImage(imageService, imagePath);
        }

        #endregion Methods
    }
}
