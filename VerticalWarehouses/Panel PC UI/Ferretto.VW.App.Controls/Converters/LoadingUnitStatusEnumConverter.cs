using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class LoadingUnitStatusEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case LoadingUnitStatus.InBay:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InBay");

                case LoadingUnitStatus.InElevator:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InElevator");

                case LoadingUnitStatus.InLocation:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InLocation");

                case LoadingUnitStatus.OnMovementToBay:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToBay");

                case LoadingUnitStatus.OnMovementToLocation:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToLocation");

                case LoadingUnitStatus.Undefined:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_Undefined");

                default:
                    return Resources.Localized.Get("InstallationApp.LoadingUnitStatus_Undefined");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
