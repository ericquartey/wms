using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.App.Menu.ViewModels;

namespace Ferretto.VW.App.Menu.Converters
{
    public class EnumInstallationStatusToIconConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case InstallationStatus.Incomplete:
                    return "CloseCircleOutline";

                case InstallationStatus.Inprogress:
                    return "ProgressCheck";

                case InstallationStatus.Complete:
                    return "CheckCircle";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
