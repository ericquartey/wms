using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class LoadingUnitStatusListEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var translate = new List<string>();
            foreach (var state in value as IEnumerable<LoadingUnitStatus>)
            {
                switch (state)
                {
                    case LoadingUnitStatus.InBay:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InBay"));
                        break;

                    case LoadingUnitStatus.InElevator:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InElevator"));
                        break;

                    case LoadingUnitStatus.InLocation:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_InLocation"));
                        break;

                    case LoadingUnitStatus.OnMovementToBay:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToBay"));
                        break;

                    case LoadingUnitStatus.OnMovementToLocation:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_OnMovementToLocation"));
                        break;

                    case LoadingUnitStatus.Undefined:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_Undefined"));
                        break;

                    default:
                        translate.Add(Resources.Localized.Get("InstallationApp.LoadingUnitStatus_Undefined"));
                        break;
                }
            }

            return translate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
