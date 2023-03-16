using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MaintenanceServiceToDescriptionConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = (Instruction)value;

            if (x != null)
            {
                if (x.Definition.Axis == Axis.Vertical)
                {
                    return OperatorApp.ElevatorVerticalAxis;
                }
                else if (x.Definition.Axis == Axis.Horizontal)
                {
                    return OperatorApp.ElevatorHorizontalAxis;
                }
                else if (x.Definition.BayNumber == BayNumber.BayOne && x.Definition.IsShutter == false)
                {
                    return OperatorApp.Bay1;
                }
                else if (x.Definition.BayNumber == BayNumber.BayOne && x.Definition.IsShutter == true)
                {
                    return OperatorApp.ShutterBay1;
                }
                else if (x.Definition.BayNumber == BayNumber.BayTwo && x.Definition.IsShutter == false)
                {
                    return OperatorApp.Bay2;
                }
                else if (x.Definition.BayNumber == BayNumber.BayTwo && x.Definition.IsShutter == true)
                {
                    return OperatorApp.ShutterBay2;
                }
                else if (x.Definition.BayNumber == BayNumber.BayThree && x.Definition.IsShutter == false)
                {
                    return OperatorApp.Bay3;
                }
                else if (x.Definition.BayNumber == BayNumber.BayThree && x.Definition.IsShutter == true)
                {
                    return OperatorApp.ShutterBay3;
                }
                else
                {
                    return OperatorApp.Machine;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
