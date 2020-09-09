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
            try
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
            catch(Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var blk = LoadingUnitStatus.Undefined;

                if (value is string strValue)
                {
                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_Undefined"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.Undefined;
                    }

                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InBay"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.InBay;
                    }

                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InElevator"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.InElevator;
                    }

                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InLocation"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.InLocation;
                    }

                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToBay"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.OnMovementToBay;
                    }

                    if (strValue.Equals(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToLocation"), StringComparison.CurrentCulture))
                    {
                        blk = LoadingUnitStatus.OnMovementToLocation;
                    }
                }

                return blk;
            }
            catch(Exception)
            {
                return LoadingUnitStatus.Undefined;
            }
        }

        #endregion
    }
}
