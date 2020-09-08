using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class LoadingUnitLocationConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case LoadingUnitLocation.CarouselBay1Down:
                        return Resources.Localized.Get("OperatorApp.CarouselBay1Down");

                    case LoadingUnitLocation.CarouselBay1Up:
                        return Resources.Localized.Get("OperatorApp.CarouselBay1Up");

                    case LoadingUnitLocation.CarouselBay2Down:
                        return Resources.Localized.Get("OperatorApp.CarouselBay2Down");

                    case LoadingUnitLocation.CarouselBay2Up:
                        return Resources.Localized.Get("OperatorApp.CarouselBay2Up");

                    case LoadingUnitLocation.CarouselBay3Down:
                        return Resources.Localized.Get("OperatorApp.CarouselBay3Down");

                    case LoadingUnitLocation.CarouselBay3Up:
                        return Resources.Localized.Get("OperatorApp.CarouselBay3Up");

                    case LoadingUnitLocation.Cell:
                        return Resources.Localized.Get("OperatorApp.Cell");

                    case LoadingUnitLocation.Down:
                        return Resources.Localized.Get("OperatorApp.Down");

                    case LoadingUnitLocation.Elevator:
                        return Resources.Localized.Get("OperatorApp.Elevator");

                    case LoadingUnitLocation.ExternalBay1Down:
                        return Resources.Localized.Get("OperatorApp.ExternalBay1Down");

                    case LoadingUnitLocation.ExternalBay1Up:
                        return Resources.Localized.Get("OperatorApp.ExternalBay1Up");

                    case LoadingUnitLocation.ExternalBay2Down:
                        return Resources.Localized.Get("OperatorApp.ExternalBay2Down");

                    case LoadingUnitLocation.ExternalBay2Up:
                        return Resources.Localized.Get("OperatorApp.ExternalBay2Up");

                    case LoadingUnitLocation.ExternalBay3Down:
                        return Resources.Localized.Get("OperatorApp.ExternalBay3Down");

                    case LoadingUnitLocation.ExternalBay3Up:
                        return Resources.Localized.Get("OperatorApp.ExternalBay3Up");

                    case LoadingUnitLocation.InternalBay1Down:
                        return Resources.Localized.Get("OperatorApp.InternalBay1Down");

                    case LoadingUnitLocation.InternalBay1Up:
                        return Resources.Localized.Get("OperatorApp.InternalBay1Up");

                    case LoadingUnitLocation.InternalBay2Down:
                        return Resources.Localized.Get("OperatorApp.InternalBay2Down");

                    case LoadingUnitLocation.InternalBay2Up:
                        return Resources.Localized.Get("OperatorApp.InternalBay2Up");

                    case LoadingUnitLocation.InternalBay3Down:
                        return Resources.Localized.Get("OperatorApp.InternalBay3Down");

                    case LoadingUnitLocation.InternalBay3Up:
                        return Resources.Localized.Get("OperatorApp.InternalBay3Up");

                    case LoadingUnitLocation.LoadUnit:
                        return Resources.Localized.Get("OperatorApp.LoadUnit");

                    case LoadingUnitLocation.NoLocation:
                        return Resources.Localized.Get("OperatorApp.NoLocation");

                    case LoadingUnitLocation.Up:
                        return Resources.Localized.Get("OperatorApp.Up");

                    default: //Unknown
                        return Resources.Localized.Get("OperatorApp.Unknown");
                }
            }
            catch(Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
