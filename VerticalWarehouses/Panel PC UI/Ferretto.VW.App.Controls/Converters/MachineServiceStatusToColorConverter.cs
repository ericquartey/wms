using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MachineServiceStatusToColorConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is MachineServiceStatus)
                {
                    switch (value)
                    {
                        case MachineServiceStatus.Valid:
                            return new SolidColorBrush(Colors.Green);

                        case MachineServiceStatus.Expired:
                            return new SolidColorBrush(Colors.Red);

                        case MachineServiceStatus.Expiring:
                            return new SolidColorBrush(Colors.Orange);

                        case MachineServiceStatus.Completed:
                            return new SolidColorBrush(Colors.White);

                        default:
                            return new SolidColorBrush(Colors.Transparent);
                    }
                }
                else
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
            }
            catch (Exception)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
