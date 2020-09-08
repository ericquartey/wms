using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ImageToSourceConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Image image)
                {
                    var ms = new MemoryStream();
                    image.Save(ms, image.RawFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    return bi;
                }
                return null;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // HACK
            return null;
        }

        #endregion
    }
}
