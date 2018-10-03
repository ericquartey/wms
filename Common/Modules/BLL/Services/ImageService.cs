using System;
using System.Configuration;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class ImageService : IImageService
    {
        #region Fields

        // TODO make the images directory configurable (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/139)
        private const string defaultImagesDirectoryName = "images\\";

        #endregion Fields

        #region Properties

        private static Uri ImageDirectoryUri =>
            new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, ConfigurationManager.AppSettings["ImagesPath"] ?? defaultImagesDirectoryName));

        #endregion Properties

        #region Methods

        public ImageSource GetImage(string pathName)
        {
            if (String.IsNullOrWhiteSpace(pathName))
            {
                return null;
            }

            var uri = new Uri(ImageDirectoryUri, pathName);

            if (!ImageDirectoryUri.IsBaseOf(uri))
            {
                throw new ArgumentException(
                    Errors.SpecifiedPathNotInConfiguredImageFolder,
                    nameof(pathName));
            }

            return File.Exists(uri.LocalPath) ? new BitmapImage(uri) : null;
        }

        #endregion Methods
    }
}
