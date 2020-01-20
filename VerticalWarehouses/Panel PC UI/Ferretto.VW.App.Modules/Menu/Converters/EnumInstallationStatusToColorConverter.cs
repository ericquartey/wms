using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.App.Menu.ViewModels;

namespace Ferretto.VW.App.Menu.Converters
{
    public class EnumInstallationStatusToColorConverter : IValueConverter
    {
        #region Properties

        public SolidColorBrush CompleteColor { get; set; }

        public SolidColorBrush IncompleteColor { get; set; }

        public SolidColorBrush InprogressColor { get; set; }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case InstallationStatus.Incomplete:
                    return this.IncompleteColor;

                case InstallationStatus.Inprogress:
                    return this.InprogressColor;

                case InstallationStatus.Complete:
                    return this.CompleteColor;
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
